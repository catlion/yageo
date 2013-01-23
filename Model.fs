module Yageo.model
open System
open System.Globalization

/// <summary>
/// На вход геокодер принимает координаты в формате “lon,lat” а на выход отдает в формате “lon lat”,
/// соответственно реализованы функции ToString и from_string
/// </summary>
type Point = {
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

// Mapping to API request params
type GeoRequest(request: string, ?max_count: byte, ?skip:byte, ?area_center: Point option, ?area_size: Point option) =
    let (!?) = fun (s: string) -> s.Trim().Replace(Environment.NewLine, String.Empty) |> System.Web.HttpUtility.UrlEncode
    let mutable geocode = request
    //let key = key
    let ll = defaultArg area_center None
    let spn = defaultArg area_size None
    let rspn = if area_center <> None && area_size <> None then 1uy else 0uy
    let results = defaultArg max_count 10uy
    let skip = defaultArg skip 0uy
    //let lang = defaultArg lang `ru-RU`
    with
        member x.Address with get() = geocode and set(v) = geocode <- v
        override x.ToString() =
            let loc = match rspn with
                      | 1uy -> sprintf "&rspn=1&ll=%s&spn=%s" (ll.Value.ToString()) (spn.Value.ToString())
                      | _ -> String.Empty
            sprintf "geocode=%s&results=%d&skip=%d&%s" !?geocode results skip !?loc

// Mapping to API response
module response =
    type Premise () =
        let mutable f = String.Empty
        member x.PremiseNumber with get() = f and set(v) = f <- v

    type Thoroughfare () =
        let mutable t = String.Empty
        let mutable p = new Premise()
        member x.ThoroughfareName with get() = t and set(v) = t <- v
        member x.Premise with get() = p and set(v) = p <- v

    type Locality () =
        let mutable l = String.Empty
        let mutable t = new Thoroughfare()
        member x.LocalityName with get() = l and set(v) = l <- v
        member x.Thoroughfare with get() = t and set(v) = t <- v

    type Country () =
        let mutable a = String.Empty
        let mutable c = String.Empty
        let mutable l = new Locality()
        member x.AddressLine with get() = a and set(v) = a <- v
        member x.CountryName with get() = c and set(v) = c <- v
        member x.Locality with get() = l and set(v) = l <- v

    type Address () =
        let mutable c = new Country()
        member x.Country with get() = c and set(v) = c <- v

    type GeoMetaData () =
        let mutable k = String.Empty
        let mutable t = String.Empty
        let mutable p = String.Empty
        let mutable a = new Address()
        member x.kind with get() = k and set(v) = k <- v
        member x.text with get() = t and set(v) = t <- v
        member x.precision with get() = p and set(v) = p <- v
        member x.AddressDetails with get() = a and set(v) = a <- v

    type Envelope () =
        let mutable l = String.Empty
        let mutable u = String.Empty
        member x.lowerCorner with get() = l and set(v) = l <- v
        member x.upperCorner with get() = u and set(v) = u <- v

    type Point () =
        let mutable p = String.Empty
        member x.pos with get() = p and set(v) = p <- v

    type Bounded () =
        let mutable e = new Envelope()
        member x.Envelope with get() = e and set(v) = e <- v

    type GeoObject1 () =
        let mutable g = new GeoMetaData()
        member x.GeocoderMetaData with get() = g and set(v) = g <- v

    type GeoObject () =
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

    type GeoObjects () =
        let mutable m = new GeoObject()
        member x.GeoObject with get() = m and set(v) = m <- v

    type Geo () =
        let mutable m = Seq.empty<GeoObjects>
        member x.featureMember with get() = m and set(v) = m <- v

    type GeoCollection () =
        let mutable col = new Geo()
        member x.GeoObjectCollection with get() = col and set(v) = col <- v

    type GeoResponse () =
        let mutable r = new GeoCollection()
        member x.response with get() = r and set(v) = r <- v
        override x.ToString() = ServiceStack.Text.JsonSerializer.SerializeToString r

// Typed API model
type Precision =
    | Exact
    | Number
    | Near
    | Street
    | Other of string
    with static member from_string = function
            | "exact" -> Exact
            | "number" -> Street
            | "near" -> Near
            | "street"  -> Street
            | x -> Other(x)

type MatchKind =
    | House
    | Street
    | Metro
    | Other of string
    with static member from_string = function
            | "house" -> House
            | "street" -> Street
            | "metro" -> Metro
            | x -> Other(x)

type GeoResult = {
    match_kind: MatchKind
    match_precision: Precision
    full_address: string
    address: string
    pos: Point
} with
    override x.ToString() = ServiceStack.Text.JsonSerializer.SerializeToString x
    static member from_result (res: response.GeoObject) =
        {
            match_kind = MatchKind.from_string res.metaDataProperty.GeocoderMetaData.kind
            match_precision = Precision.from_string res.metaDataProperty.GeocoderMetaData.precision
            full_address = res.metaDataProperty.GeocoderMetaData.text
            address = res.metaDataProperty.GeocoderMetaData.AddressDetails.Country.AddressLine
            pos = Point.from_string res.Point.pos
        }
