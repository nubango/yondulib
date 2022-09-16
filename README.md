# yondulib

## System Requirements

- Unity 2020.3.5f1 or later
- Intel 64-bit desktop platforms (Windows, macOS, Linux)

## How To Install

This package uses the [scoped registry](https://docs.unity3d.com/Manual/upm-scoped.html) feature to resolve package
dependencies. Please add the following lines to the manifest file
(`Packages/manifest.json`).

<details>
<summary>.NET Standard 2.0 (Unity 2021.1 or earlier)</summary>

To the `scopedRegistries` section:

```
"scopedRegistries": [
    {
      "name": "Unity NuGet",
      "url": "https://unitynuget-registry.azurewebsites.net",
      "scopes": [
        "org.nuget"
      ]
    },
    {
      "name": "Keijiro",
      "url": "https://registry.npmjs.com",
      "scopes": [
        "jp.keijiro"
      ]
    }
  ]
```
</details>

<details>
<summary>.NET Standard 2.1 (Unity 2021.2 or later)</summary>

To the `scopedRegistries` section:

```
"scopedRegistries": [
    {
      "name": "Keijiro",
      "url": "https://registry.npmjs.com",
      "scopes": [
        "jp.keijiro"
      ]
    }
  ]
```
</details>

---

Use Unity Package Manager to install the package via the following git URL: `https://github.com/nubango/yondulib.git`

![GIF Unity Package by URL](https://i.gyazo.com/b54e9daa9a483d9bf7f74f0e94b2d38a.gif)

## How to download FPS Demo

The plugin contains an example of ```yondulib``` usage that you can download into your project from the _Package Manager_ window of _Unity_. The sample is called _FPS Shooter Demo_ and contains a test scene where you control a character in first person. You move forward with whistles and shoot with finger clicks or claps.

![Demo Image](https://user-images.githubusercontent.com/27202047/190650897-7595492d-9f85-473f-8d9c-36b50995eb82.png)

To change the controls that make the actions, just open the _SimpleControls_ file (_Input Action Asset_) and add or remove controls in the action editor.

![Input Action Asset Image](https://i.gyazo.com/f4b76ab763d25a6c1f74718b94549ded.png)

## Additional information

This repository, together with that of the complete project ([yondulib-project](https://github.com/nubango/yondulib-project)) is part of the Final Degree Project: "Yondulib: Tool for using sounds as inputs for Unity videogames".

Created by Gonzalo Alba Durán and Nuria Bango Iglesias for the Complutense University of Madrid, directed by Manuel Freire Morán.
