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
        ' JW Player on page, simple JavaScript init
        Await TestSimple("http://reslife.uww.edu/stream/", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function JWPlayerIframe() As Task
        ' JW Platform iframe embed
        Await TestSimple("https://www.uwec.edu/LTS/services/media/streaming/footbridge.htm", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function Cablecast1() As Task
        ' Requires application/x-mpegurl in accept header; returns a content-type of application/x-mpegURL (notice uppercase)
        Await TestSimple("http://watchictv.org/live-streaming", "application/x-mpegURL")
    End Function

    <TestMethod()> Public Async Function Cablecast2() As Task
        ' Requires application/x-mpegurl in accept header; returns a content-type of application/x-mpegURL (notice uppercase)
        Await TestSimple("http://haverhillcommunitytv.org/video/channel-9-live-stream", "application/x-mpegURL")
    End Function

    <TestMethod()> Public Async Function WiscCesa10() As Task
        ' These school district streams are usually offline (trying to get the .m3u8 returns 404)
        Dim url = "https://ensemble.cesa10.k12.wi.us/Watch/Gq48RnWz"
        For i = 0 To 10
            Dim result = Await Resolver.ResolveAsync(url)
            If result.Links.Any Then
                url = result.Links.First.Url
                If url.EndsWith(".m3u8") Then
                    Return
                End If
            End If
        Next
        Assert.Fail("Did not find a .m3u8 URL")
    End Function

    <TestMethod()> Public Async Function Temple() As Task
        ' Ensemble Video
        Await TestSimple("http://templetv.net/watch-live/", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function NorthernArizona() As Task
        ' Seems to be video.js
        Await TestSimple("http://nau-tv.com/watchlive/", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function StretchInternetVOD() As Task
        ' This site has special behavior - see Resolver.cs
        Await TestSimple("https://portal.stretchinternet.com/kwu/portal.htm?eventId=444970&streamType=video", "video/mp4")
    End Function

    <TestMethod()> Public Async Function WBAY() As Task
        ' JW Player on page
        Await TestSimple("http://www.wbay.com/livestream", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function WowzaPlayer() As Task
        ' Wowza Player
        Await TestSimple("https://www.wowza.com/blog/webinar-how-stetson-university-met-the-challenges-of-providing-campus-wide", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function SanDiegoZoo() As Task
        ' iframe, throws 406 if text/html not acceptable
        Dim result1 = Await Resolver.ResolveAsync("http://zoo.sandiegozoo.org/cams/panda-cam")
        Dim link1 = result1.Links.Where(Function(l) Not l.Url.Contains("googletagmanager")).Single
        Dim result2 = Await Resolver.ResolveAsync(link1.Url)
        For Each link2 In result2.Links
            Await TestSimple(link2.Url, "application/vnd.apple.mpegurl")
        Next
    End Function

    <TestMethod()> Public Async Function SingleMP4() As Task
        ' Anvato
        Await TestSimple("http://www.html5videoplayer.net/html5video/mp4-h-264-video-test/", "video/mp4")
    End Function

    <TestMethod()> Public Async Function MultipleMP4() As Task
        ' Multiple <video> tags on one page
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