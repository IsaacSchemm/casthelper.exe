# Cast Helper

Download: https://github.com/IsaacSchemm/casthelper.exe/releases
Screenshots: https://imgur.com/a/j3lRBfE

Cast Helper is a small .NET Framework / Windows Forms application that lets you
send media URLs to a Roku or Apple TV on your local area network.

Both Roku and Apple TV devices will detect the format of a video on their own,
so any video format that the device supports should work. Cast Helper also
supports sending .mp3 and .m4a audio to Roku devices. Photos are not currently
supported on either device.

You can pass a URL as a command-line argument to casthelper.exe to pre-fill
the URL box. The media won't start automatically, because the user still needs
to select a device.

## HTTP behavior

When the user clicks Play, Cast Helper will make an HTTP HEAD request to the
given URL to determine the type of the data. If it encounters a redirect, it
will follow the Location header and update the URL text box accordingly.
Cast Helper will also remember cookies (as long as it is running).

HTTP errors will result in an error message. There are unique error messages
for HTTP 404 Not Found, HTTP 406 Not Acceptable, and HTTP 410 Gone. The user
will also get an error when the content type does not begin with `audio/` or
`video/` and is not an HLS, DASH, or Smooth Streaming manifest.

The request headers sent by Cast Helper are:

    User-Agent: CastHelper/1.2 (https://github.com/IsaacSchemm/casthelper.exe)
	Accept: application/vnd.apple.mpegurl,application/x-mpegurl,application/dash+xml,application/vnd.ms-sstr+xml,video/*,audio/*

(You can use these headers to redirect to raw video or audio content when you
might normally return a web page. Note that text/html is missing from the
Accept header.)

To allow the user a choice of multiple items in Cast Helper 1.2+, have your
server return an M3U mformatted playlist as an HTTP 300 response with a
content type of audio/mpegurl. (Do not use an HTTP 200 status code - Cast
Helper will assume the file is an HLS stream.)

If Cast Helper 1.2+ does recieve an HTML response, it will be scanned for MP4
and HLS video URLs, and (if none exist) iframe URLs. If more than one is
found, the user will be asked which URL they want to follow. You shouldn't
rely on this behavior - it will probably fail on the majority of websites -
but it might be helpful in certain situations.

Once the media is sent to the device, Cast Helper's main window will close.
For Apple TV devices, another window will open to monitor playback; closing
this window will stop the video on the Apple TV. (Roku devices continue to
play on their own once the video is started.)
