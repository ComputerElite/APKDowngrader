# APKDowngrader
And program to create and use downgrade/update files for apks

## How does it work?
It XORs the 2 apks to get the differences. Then it just XORs your apk with the downgrading file and you get a downgraded apk without ever doenloading it's code. For more info read [this wikipedia article](https://en.wikipedia.org/wiki/One-time_pad)

## How do I downgrade when I have an APK?
1. Open the program
2. Select an APK you want to downgrade via `Choose APK`
3. Put in the target version.
4. Now either check if you can downgrade or click start downgrade (both will check).
You now have an APK Named `<Package ID>_<Target Version>.apk` next to the exe you can install

## How do I downgrade with one click?
1. Open the program and close it
2. Now open `appid.txt` next to the exe and edit it's text to the package ID you want t downgrade (default one is `com.beatgames.beatsaber` for Beat Saber)
3. Open the program again and connect your Android Device (e. g. Oculus Quest, Android Phone)
4. Put in your target version
6. Press `Auto Downgrade`
Now it'll downgrade and install.

## How do I generate downgrade files?
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
You can find some links on https://github.com/ComputerElite/wiki/notthereyet . Just download the file and place it next to your exe (if you downloaded a zip file then first extract the zip)
