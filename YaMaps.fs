module Yageo.map
open System
open System.Net
open Yageo.model
open ServiceStack.Text

let geocode (p: GeoRequest) = async {
    use wc = new WebClient()
    wc.UseDefaultCredentials <- true
    let! res = wc.AsyncDownloadString(new Uri("http://geocode-maps.yandex.ru/1.x/?format=json&" + p.ToString()))
    return JsonSerializer.DeserializeFromString<
}