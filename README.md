# Cast Helper

Download: https://github.com/IsaacSchemm/casthelper.exe/releases

Cast Helper is a small portable Windows application that lets you cast video
from the Internet to a Roku or Apple TV on your local area network. It also
supports sending .mp3 and .m4a audio to Roku devices.

## How to use

1. Download casthelper.exe from the [releases page](https://github.com/IsaacSchemm/casthelper.exe/releases) and run it.

2. Cast Helper will search for available Apple TV and Roku devices on your
   network. Choose the device you want to cast to from the drop-down list. (If
   your Roku does not appear in the list, you can also enter its IP address
   manually from a menu in the toolbar.)

3. Paste the media URL into the URL text box.

4. Click the Play button.

## For developers

When the user clicks Play, Cast Helper will make an HTTP HEAD request to the
given URL to determine the type of the data.

HTTP errors will result in an error message. There are unique error messages
for HTTP 404 Not Found, HTTP 406 Not Acceptable, and HTTP 410 Gone. The user
will also get an error when the content type does not begin with `audio/` or
`video/` (and is not a recognized HLS content type, either).

The request headers sent by Cast Helper are:

    User-Agent: CastHelper/2.0 (https://github.com/IsaacSchemm/casthelper.exe)
	Accept: application/vnd.apple.mpegurl,video/*,audio/*

If you are developing a web application and you want your users to be able to
use Cast Helper, I would suggest looking at the Accept header and checking if
the client accepts the video type you're using, but not text/html.
