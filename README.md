# casthelper.exe

CastHelper is a small .NET Framework / Windows Forms application that lets you
send media URLs to a Roku or Apple TV on your local area network.

Both Roku and Apple TV devices will detect the format of a video on their own,
so any video format that the device supports should work. CastHelper also
supports sending .mp3 and .m4a audio to Roku devices.

## HTTP behavior

When the user clicks Play, CastHelper will make an HTTP HEAD request to the
given URL to determine the type of the data. If it encounters a redirect, it
will follow the Location header and update the URL text box accordingly.
CastHelper will also remember cookies (as long as it is running).

HTTP errors will result in an error message. There are unique error messages
for HTTP 404 Not Found and HTTP 410 Gone. The user will also get an error when
the content type does not begin with `audio/` or `video/` and is not an HLS,
DASH, or Smooth Streaming manifest.

The request headers sent by CastHelper are:

    User-Agent: CastHelper/1.0 (https://github.com/IsaacSchemm/casthelper.exe)
	Accept: application/vnd.apple.mpegurl,application/dash+xml,application/vnd.ms-sstr+xml,video/*,audio/*

(You can use these headers to redirect to raw video or audio content when you
might normally return a web page. Note that text/html is missing from the
Accept header.)
