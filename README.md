# APKDowngrader
And program to create and use downgrade/update files for apks

## How does it work?
It XORs the 2 apks to get the differences. Then it just XORs your apk with the downgrading file and you get a downgraded apk without ever doenloading it's code. For more info read [this wikipedia article](https://en.wikipedia.org/wiki/One-time_pad)

## Usage
### How do I downgrade when I have an APK?
1. Open the program
2. Select an APK you want to downgrade via `Choose APK`
3. Put in the target version.
4. Now either check if you can downgrade or click start downgrade (both will check).
You now have an APK Named `<Package ID>_<Target Version>.apk` next to the exe you can install

### How do I downgrade with one click?
1. Open the program and close it
2. Now open `appid.txt` next to the exe and edit it's text to the package ID you want t downgrade (default one is `com.beatgames.beatsaber` for Beat Saber)
3. Open the program again and connect your Android Device (e. g. Oculus Quest, Android Phone)
4. Put in your target version
6. Press `Auto Downgrade`
Now it'll downgrade and install.

### How do I generate downgrade files?
1. Get 2 apks (the one you want to downgrade and the version you want the downgraded one to be)
2. Open the program
3. Hold shift while clicking `Choose APK`
4. Select the apks
5. Click Start Downgrade
A file named `<Package ID>_<VERSION>TO<VERSION>.decr` will be nextto the exe and in versions.json will now be metadata to the downgrade file

## Errors
### `The Version downgrade isn't available for those versions.`
This means the downgrading of this app and version to the target version hasn't been checked by trusted people.
It is possible by adding a version to `versions.json` (For that you need some knowledge).

### `You haven't got <filename> to downgrade your App.`
This means the downgrading of this app and version to the target version has been checked by trusted people but you don't have the downgrading file.
You should get a prompt where you can download the file if the download link has been added to the downgrade entry.

### `File is used by another process` aka long fat error
Restart the program, selct and apk and then **wait 30 seconds before starting to downgrade**. The Error might not come anymore.
_This error shouldn't appear anymore_

### `I'm sorry. Due to the source file having to be as big as the target one or bigger to not distribute game code I can't do that for you`
Due to XOR-ing the files to generate downgrade files it is not possible to go from a e. g. 400MB file to a 500 MB file. As if so the downgrade file had to be 500 MB. But then the question comes how do I get from 400MB to 500MB without adding game code? Well the answer would be copying bytes. But I do not plan to implement that and just kepp this limitation.

## Issues
- N/A
