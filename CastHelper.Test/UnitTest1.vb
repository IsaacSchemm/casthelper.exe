Imports System.Net
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()> Public Class UnitTest1

    Private Async Function TestSimple(url As String, expected As String) As Task
        Dim cookieContainer As New CookieContainer
        For i = 0 To 10
            Dim result = Await Resolver.ResolveAsync(url, cookieContainer)
            If result.Links.Any Then
                url = result.Links.Single.Url
            ElseIf result.ContentType IsNot Nothing Then
                Assert.AreEqual(expected, result.ContentType)
                Exit For
            End If
        Next
    End Function

    <TestMethod()> Public Async Function JWPlayer() As Task
        Await TestSimple("http://reslife.uww.edu/stream/", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function JWPlayerIframe() As Task
        Await TestSimple("https://www.uwec.edu/LTS/services/media/streaming/footbridge.htm", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function Cablecast1() As Task
        Await TestSimple("http://watchictv.org/live-streaming", "application/x-mpegURL")
    End Function

    <TestMethod()> Public Async Function Cablecast2() As Task
        Await TestSimple("http://haverhillcommunitytv.org/video/channel-9-live-stream", "application/x-mpegURL")
    End Function

    <TestMethod()> Public Async Function Temple() As Task
        Await TestSimple("http://templetv.net/watch-live/", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function NorthernArizona() As Task
        Await TestSimple("http://nau-tv.com/watchlive/", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function StretchInternetVOD() As Task
        Await TestSimple("https://portal.stretchinternet.com/kwu/portal.htm?eventId=444970&streamType=video", "video/mp4")
    End Function

    <TestMethod()> Public Async Function WBAY() As Task
        Await TestSimple("http://www.wbay.com/livestream", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function WowzaPlayer() As Task
        Await TestSimple("https://www.wowza.com/blog/webinar-how-stetson-university-met-the-challenges-of-providing-campus-wide", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function WFRV_VOD() As Task
        Await TestSimple("http://www.wearegreenbay.com/local-5-live/local-5-live-recipes/smoked-salmon-cucumber-flatbread/1187259490", "video/mp4")
    End Function

    <TestMethod()> Public Async Function SingleMP4() As Task
        Await TestSimple("http://www.html5videoplayer.net/html5video/mp4-h-264-video-test/", "video/mp4")
    End Function

    <TestMethod()> Public Async Function MultipleMP4() As Task
        Dim result1 = Await Resolver.ResolveAsync("https://developer.mozilla.org/en-US/docs/Web/HTML/Element/video")
        If result1.Links.Count < 2 Then
            Assert.Inconclusive()
        End If
        For Each link In result1.Links
            Await TestSimple(link.Url, "video/mp4")
        Next
    End Function

    <TestMethod()> Public Async Function M3u8Disambiguation() As Task
        Dim result1 = Await Resolver.ResolveAsync("https://www.lakora.us/casthelper/300.m3u8.php")
        Assert.AreEqual("audio/mpegurl", result1.ContentType)
        Assert.AreEqual(2, result1.Links.Count)
        Assert.AreEqual(result1.Links(0).Name, "An MP4 file")
        Assert.AreEqual(result1.Links(0).Url, "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4")
        Assert.AreEqual(result1.Links(1).Name, "An HLS live stream")
        Assert.AreEqual(result1.Links(1).Url, "https://devstreaming-cdn.apple.com/videos/streaming/examples/img_bipbop_adv_example_ts/v5/prog_index.m3u8")
    End Function

    <TestMethod()> Public Async Function HtmlDisambiguation() As Task
        Dim result1 = Await Resolver.ResolveAsync("https://www.lakora.us/casthelper/300.html.php")
        Assert.IsTrue(result1.ContentType.StartsWith("text/html"))
        Assert.AreEqual(2, result1.Links.Count)
        Assert.AreEqual(result1.Links(0).Url, "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4")
        Assert.AreEqual(result1.Links(1).Url, "https://devstreaming-cdn.apple.com/videos/streaming/examples/img_bipbop_adv_example_ts/v5/prog_index.m3u8")
    End Function

End Class