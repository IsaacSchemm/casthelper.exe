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

    <TestMethod()> Public Async Function StretchInternet() As Task
        Await TestSimple("https://portal.stretchinternet.com/kwu/portal.htm?eventId=444970&streamType=video", "video/mp4")
    End Function

    <TestMethod()> Public Async Function WBAY() As Task
        Await TestSimple("http://www.wbay.com/livestream", "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function WFRV_VOD() As Task
        Await TestSimple("http://www.wearegreenbay.com/local-5-live/local-5-live-recipes/smoked-salmon-cucumber-flatbread/1187259490", "video/mp4")
    End Function

    <TestMethod()> Public Async Function Livestream() As Task
        Dim cookieContainer As New CookieContainer
        Dim result1 = Await Resolver.ResolveAsync("http://ktla.com/on-air/live-streaming/", cookieContainer)
        Assert.IsTrue(result1.Links.Count() > 1)
        Await TestSimple(result1.Links.Where(Function(s) Not s.Url.Contains("googletagmanager")).First.Url, "application/vnd.apple.mpegurl")
    End Function

    <TestMethod()> Public Async Function Html5VideoPlayerNet() As Task
        Await TestSimple("http://www.html5videoplayer.net/html5video/mp4-h-264-video-test/", "video/mp4")
    End Function

End Class