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

    <TestMethod()> Public Async Function NorthernArizona() As Task
        Await TestSimple("http://nau-tv.com/watchlive/", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function StretchInternetVOD() As Task
        Await TestSimple("https://portal.stretchinternet.com/kwu/portal.htm?eventId=444970&streamType=video", "video/mp4")
    End Function

    <TestMethod()> Public Async Function WBAY() As Task
        Await TestSimple("http://www.wbay.com/livestream", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function VideoJS_HLS() As Task
        Dim cookieContainer As New CookieContainer
        Dim result1 = Await Resolver.ResolveAsync("https://videojs.github.io/videojs-contrib-hls/", cookieContainer)
        For Each link In result1.Links
            If Not link.Url.Contains("example.com") Then
                Await TestSimple(link.Url, "application/vnd.apple.mpegurl")
            End If
        Next
    End Function

    <TestMethod()> Public Async Function WFRV_VOD() As Task
        Await TestSimple("http://www.wearegreenbay.com/local-5-live/local-5-live-recipes/smoked-salmon-cucumber-flatbread/1187259490", "video/mp4")
    End Function

    <TestMethod()> Public Async Function SingleMP4() As Task
        Await TestSimple("http://www.html5videoplayer.net/html5video/mp4-h-264-video-test/", "video/mp4")
    End Function

    <TestMethod()> Public Async Function MultipleMP4() As Task
        Dim cookieContainer As New CookieContainer
        Dim result1 = Await Resolver.ResolveAsync("https://developer.mozilla.org/en-US/docs/Web/HTML/Element/video", cookieContainer)
        If result1.Links.Count < 2 Then
            Assert.Inconclusive()
        End If
        For Each link In result1.links
            Await TestSimple(link.Url, "video/mp4")
        Next
    End Function

End Class