module Yageo.model
open System
type Point = {
    lat: double
    lon: double
} with override x.ToString() =
    x.lon.ToString("F", System.Globalization.CultureInfo.InvariantCulture)
    + ", "
    + x.lat.ToString("F", System.Globalization.CultureInfo.InvariantCulture)

type Address = {
    country: string
    city: string
    street: string
    premise: string
}

let (!?) = fun (x: string) -> System.Web.HttpUtility.UrlEncode x

type GeoRequest(request: string, key: string, ?max_count: byte, ?skip:byte, ?area_center: Point option, ?area_size: Point option) =
    let geocode = request
    let key = key
    let ll = defaultArg area_center None
    let spn = defaultArg area_size None
    let rspn = if area_center <> None && area_size <> None then 1uy else 0uy
    let results = defaultArg max_count 10uy
    let skip = defaultArg skip 0uy
    //let lang = defaultArg lang `ru-RU`
    with
        override x.ToString() =
            let loc = match rspn with
                      | 1uy -> sprintf "&rspn=1&ll=%s&spn=%s" (ll.Value.ToString()) (spn.Value.ToString())
                      | _ -> String.Empty
            sprintf "geocode=%s&key=%s&results=%d&skip=%d&%s" !?geocode !?key results skip !?loc

type GeoResponse() =
    member x.