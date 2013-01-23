module Yageo.map
open System
open System.Text.RegularExpressions
open System.Net
open Yageo.model
open ServiceStack.Text

let url (p: GeoRequest) =
    new Uri("http://geocode-maps.yandex.ru/1.x/?format=json&" + p.ToString())

// API manual: http://api.yandex.ru/maps/doc/jsapi/2.x/ref/reference/geocode.xml

let rec AsyncGeocode (p: GeoRequest) = async {
    use wc = new WebClient()
    let u = url p
    let! res = wc.AsyncDownloadString u
    let parsed = JsonSerializer.DeserializeFromString<response.GeoResponse>(res)
    return parsed.response.GeoObjectCollection.featureMember
            |> Seq.map (fun x -> GeoResult.from_result x.GeoObject)
}

let Geocode (p: GeoRequest) =
    use wc = new WebClient()
    let res = wc.DownloadString(url p)
    let parsed = JsonSerializer.DeserializeFromString<response.GeoResponse>(res)
    parsed.response.GeoObjectCollection.featureMember
        |> Seq.map (fun x -> GeoResult.from_result x.GeoObject)