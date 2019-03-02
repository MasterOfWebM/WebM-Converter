# WebM.y.TsM's WebM Converter

This is a simple wrapper for [FFmpeg](http://www.ffmpeg.org/download.html) to create WebM files. My version is generally among the best, due to my experience with encoding videos for years. To download it, please click [here](https://github.com/MasterOfWebM/WebM-Converter/releases/download/v1.1/Release_v1.1.zip).

Make sure that you have your own FFmpeg in the root directory of the program, otherwise nothing will happen. Better yet, FFmpeg should be within your System Variable, but whatever.

## What can it do?
This is what she looks like:

![alt text](http://i.imgur.com/ByZBrvV.png)

Currently she can:

* Automatically calculate the bitrate needed for a given size
* Checks the size of the outputted video to see if it worked properly
  * If it didn't, then it gives recommendations to fix the file size
* Ability to use subtitles
* Update check
* Scale the video
* Crop the video
* Seek to a custom time
* Obtain incredibly close results in file size (currently limited support)

## Why only offer 2-pass?
Currently it is only offered since this is designed as a GUI for 4chan users. 2-pass encoding will look better than crf with q-min/max 95% of the time, since 2-pass is designed to get the best quality for a given size. In the future, I wish to make a complete WebM wrapper, and even maybe a complete FFmpeg wrapper.

## Can you explain what the weird flags are?
Sure!

* -quality best: Should be obvious. You can use "-quality good -cpu-used 0" to speed it up, but the results will looks worse.
* -slices 8: Slices make the encoder cut the individual frames into the amount of slices (1, 2, 4, & 8 are possible). Generally 4 is good enough for 720p content. Slices slightly increase the quality of each frame.
* -auto-alt-ref 1: This enables the encoder to look into the future instead of just the past, or "Golden Frame". This can increase the quality drastically, or severely hurt it. It works 99% of the time, so I just leave it on.
* -lag-in-frames 16: This is amount of frames it can look into the future. The limit is around 25, but 16 seems to be the best quality/speed tradeoff point on lower-end machines.

## Why a speaker icon?
Irony.

## What is planned?
Plenty of stuff is planned:

* Drag and Drop
* Preview (general and crop)
* Plenty of more stuff

## License
The code is released under [the GPLv2 license](http://www.gnu.org/licenses/gpl-2.0.html).
