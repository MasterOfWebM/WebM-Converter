#!WebM.y.TsM's WebM Converter
This is a simple wrapper for [ffmpeg](http://www.ffmpeg.org/download.html) to create WebM files.

Make sure that you have your own ffmpeg in the root directory of the program, otherwise nothing will happen. Better yet, it should be within your System Variable, but whatever.

##What can it do?
This is what she looks like:

![alt text](http://i.imgur.com/PKGlv2I.png)

Currently it can only:

* Automatically calculate the bitrate needed (3MB (Which doesn't work for all boards, like /b/))
* Scale the video
* Seek to a custom time

##What is planned?
Plenty of stuff is planned:

* Ability to use subtitles
* Input custom size restriction
* Check the size of the outputted video to see if it worked properly
  * If it didn't, then give recommendations to the user to fix it
* Update check
* Drag and Drop
* Plenty of more stuff

##License
The code is released under [the GPLv2 license](http://www.gnu.org/licenses/gpl-2.0.html).