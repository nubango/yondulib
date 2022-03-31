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

![](https://i.gyazo.com/b54e9daa9a483d9bf7f74f0e94b2d38a.gif)

