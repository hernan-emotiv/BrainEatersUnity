# Local Unity Packages

Place local Unity package tarballs here when they are not available through a package registry.

Required for Cortex SDK v5:

```text
PackagesLocal/com.emotiv.cortex-5.0.0-release.8.tgz
```

Do not commit `.tgz` files. They are large and may contain SDK binaries distributed outside this repository.

After placing the tarball, Unity resolves it from:

```json
"com.emotiv.cortex": "file:../PackagesLocal/com.emotiv.cortex-5.0.0-release.8.tgz"
```

