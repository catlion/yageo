﻿namespace Yageo
open System
open System.Net
open System.Globalization
open ServiceStack.Text

module AssemblyAttributes =
    [<assembly: System.Runtime.InteropServices.ComVisible(false);
      assembly: System.CLSCompliant(true)>]
    do()

/// <summary>
/// На вход геокодер принимает координаты в формате “lon,lat” а на выход отдает в формате “lon lat”,
/// соответственно реализованы функции ToString и from_string
/// </summary>
type Coords = {
    lon: double
    lat: double
} with
    override x.ToString() =
        x.lon.ToString("F", CultureInfo.InvariantCulture) +
            "," +
            x.lat.ToString("F", CultureInfo.InvariantCulture)
    static member from_string (s: string) =
        let a = s.Split(' ')
        {
            lon = Double.Parse(a.[0], CultureInfo.InvariantCulture)
            lat = Double.Parse(a.[1], CultureInfo.InvariantCulture)
        }

/// <summary>
/// Параметры запроса к геокодеру
/// </summary>
type GeoRequest(request: string, maxCount: byte, skip:byte, areaCenter: Coords option, areaSize: Coords option) =
    let (!?) (s: string) = s.Trim().Replace(Environment.NewLine, String.Empty) |> System.Web.HttpUtility.UrlEncode
    let mutable geocode = request
    let rspn = if areaCenter <> None && areaSize <> None then 1uy else 0uy
    with
        member x.Address with get() = geocode and set(v) = geocode <- v
        override x.ToString() =
            let loc = match rspn with
                      | 1uy -> sprintf "&rspn=1&ll=%s&spn=%s" (areaCenter.ToString()) (areaSize.ToString())
                      | _ -> String.Empty
            sprintf "geocode=%s&results=%d&skip=%d&%s" !?geocode maxCount skip !?loc
        member private x.Url with get() = new Uri("http://geocode-maps.yandex.ru/1.x/?format=json&" + x.ToString())
        member private x.Parse res =
            let parsed = JsonSerializer.DeserializeFromString<GeoResponse>(res)
            let resp: GeoCollection = parsed.response
            let gcol: Geo = resp.GeoObjectCollection
            gcol.featureMember
        member private x.Convert (i: GeoObjects) =
            GeoResult.from_result i.GeoObject
        member x.Geocode () =
            use wc = new WebClient()
            x.Parse <| wc.DownloadString x.Url |> Seq.map x.Convert
        member x.AsyncGeocode () = async {
            use wc = new WebClient()
            let! res = wc.AsyncDownloadString x.Url
            let parsed = x.Parse res
            return parsed |> Seq.map x.Convert
        }
        new(request, maxCount, skip) =
            GeoRequest(request, maxCount, skip, None, None)
        new(request) =
            GeoRequest(request, 10uy, 0uy)
and Premise () =
    let mutable f = String.Empty
    member x.PremiseNumber with get() = f and set(v) = f <- v
and Thoroughfare () =
    let mutable t = String.Empty
    let mutable p = new Premise()
    member x.ThoroughfareName with get() = t and set(v) = t <- v
    member x.Premise with get() = p and set(v) = p <- v
and Locality () =
    let mutable l = String.Empty
    let mutable t = new Thoroughfare()
    member x.LocalityName with get() = l and set(v) = l <- v
    member x.Thoroughfare with get() = t and set(v) = t <- v
and Country () =
    let mutable a = String.Empty
    let mutable c = String.Empty
    let mutable l = new Locality()
    member x.AddressLine with get() = a and set(v) = a <- v
    member x.CountryName with get() = c and set(v) = c <- v
    member x.Locality with get() = l and set(v) = l <- v
and Address () =
    let mutable c = new Country()
    member x.Country with get() = c and set(v) = c <- v
and GeoMetaData () =
    let mutable k = String.Empty
    let mutable t = String.Empty
    let mutable p = String.Empty
    let mutable a = new Address()
    member x.kind with get() = k and set(v) = k <- v
    member x.text with get() = t and set(v) = t <- v
    member x.precision with get() = p and set(v) = p <- v
    member x.AddressDetails with get() = a and set(v) = a <- v
and Envelope () =
    let mutable l = String.Empty
    let mutable u = String.Empty
    member x.lowerCorner with get() = l and set(v) = l <- v
    member x.upperCorner with get() = u and set(v) = u <- v
and Point () =
    let mutable p = String.Empty
    member x.pos with get() = p and set(v) = p <- v
and Bounded () =
    let mutable e = new Envelope()
    member x.Envelope with get() = e and set(v) = e <- v
and GeoObject1 () =
    let mutable g = new GeoMetaData()
    member x.GeocoderMetaData with get() = g and set(v) = g <- v
and GeoObject () =
    let mutable o = new GeoObject1()
    let mutable d = String.Empty
    let mutable n = String.Empty
    let mutable b = new Bounded()
    let mutable p = new Point()
    member x.metaDataProperty with get() = o and set(v) = o <- v
    member x.description with get() = d and set(v) = d <- v
    member x.name with get() = n and set(v) = n <- v
    member x.boundedBy with get() = b and set(v) = b <- v
    member x.Point with get() = p and set(v) = p <- v
and GeoObjects () =
    let mutable m = new GeoObject()
    member x.GeoObject with get() = m and set(v) = m <- v
and Geo () =
    let mutable m = Seq.empty<GeoObjects>
    member x.featureMember with get() = m and set(v) = m <- v
and GeoCollection () =
    let mutable col = new Geo()
    member x.GeoObjectCollection with get() = col and set(v) = col <- v
and GeoResponse () =
    let mutable r = new GeoCollection()
    member x.response with get() = r and set(v) = r <- v
    override x.ToString() = ServiceStack.Text.JsonSerializer.SerializeToString r

// Typed API model
and Precision =
    | Exact
    | Number
    | Near
    | Street
    | Other of string
    with static member from_string = function
            | "exact" -> Exact
            | "number" -> Number
            | "near" -> Near
            | "street"  -> Street
            | x -> Other(x)
and MatchKind =
    | House
    | Street
    | Metro
    | Other of string
    with static member from_string = function
            | "house" -> House
            | "street" -> Street
            | "metro" -> Metro
            | x -> Other(x)
and GeoResult = {
    match_kind: MatchKind
    match_precision: Precision
    full_address: string
    address: string
    pos: Coords
} with
    override x.ToString() = ServiceStack.Text.JsonSerializer.SerializeToString x
    static member from_result (res: GeoObject) =
        {
            match_kind = MatchKind.from_string res.metaDataProperty.GeocoderMetaData.kind
            match_precision = Precision.from_string res.metaDataProperty.GeocoderMetaData.precision
            full_address = res.metaDataProperty.GeocoderMetaData.text
            address = res.metaDataProperty.GeocoderMetaData.AddressDetails.Country.AddressLine
            pos = Coords.from_string res.Point.pos
        }
