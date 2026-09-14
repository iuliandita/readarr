# Readarr (maintenance fork)

> [!NOTE]
> This is an actively maintained fork of [Readarr](https://github.com/Readarr/Readarr). Upstream retired the project, so this fork exists to keep existing installs secure and usable: security fixes, dependency and runtime repair, breaking bug fixes, packaging and container work, and metadata-provider survivability. It also ports selected fixes and improvements from [Sonarr](https://github.com/Sonarr/Sonarr), which shares this project's core, and from other forks, where they make sense for books. It is not a ground-up rewrite and does not try to match Readarr's original scope feature for feature.

## What this fork does

- Ships security fixes, including issues upstream never patched.
- Keeps dependencies and the .NET runtime current and buildable.
- Fixes breaking bugs in imports, downloads, the organizer, and the API.
- Keeps metadata working through pluggable metadata sources.
- Backports selected fixes from Sonarr's shared core.
- Publishes a LinuxServer-compatible container image for existing homelab installs.

The current backlog, including tracked security work and upstream ports, lives in the [issue tracker](https://github.com/iuliandita/readarr/issues).

## Container image

Images are built on the [LinuxServer.io base image](https://github.com/linuxserver/docker-baseimage-alpine) and keep its s6 / PUID-PGID layout, so they drop into existing homelab setups ([LinuxServer docs](https://docs.linuxserver.io/)). Tagged releases publish to `ghcr.io/iuliandita/readarr:<version>`; the `develop` tag tracks the development branch.

## Metadata provider

The original Readarr metadata server (`api.bookinfo.club`) is gone (the domain no longer resolves), which left a clean install unable to search, add, or refresh books. To keep the app usable out of the box, this fork defaults the metadata source to the hosted [rreading-glasses](<https://github.com/blampe/rreading-glasses>) public instance at `https://api.bookinfo.pro` (Goodreads-backed).

- **Third party, unsupported, use at your own risk.** It is a free community service run by others; this fork is not affiliated with it and cannot support it. Expect rate limits and possible downtime, and review its terms before relying on it.
- **Overridable.** Change it under **Settings -> Development -> Metadata Source**. For Hardcover-backed metadata, use `https://hardcover.bookinfo.pro`. Clear the field to fall back to the (now dead) built-in default.
- **Self-hosting (recommended for reliability).** Run your own with the `blampe/rreading-glasses:latest` image and point the override at it, e.g. `http://rreading-glasses:8788`.
- **Caveat:** rreading-glasses formats some titles differently from the old server. Existing libraries may need a re-import/refresh of works with long subtitles.

## Background

Upstream [Readarr](https://github.com/Readarr/Readarr) was retired in 2025 after its metadata server stopped working and the effort to move to another source stalled. The full original announcement and its key points remain available in the upstream repository history. This fork continues maintenance of the code base so existing libraries keep working, and it does not depend on upstream for metadata.

## Major features

* Can watch for better quality of the ebooks and audiobooks you have and do an automatic upgrade. *e.g. from PDF to AZW3*
* Support for major platforms: Windows, Linux, macOS, Raspberry Pi, etc.
* Automatically detects new books
* Can scan your existing library and download any missing books
* Automatic failed download handling will try another release if one fails
* Manual search so you can pick any release or to see why a release was not downloaded automatically
* Advanced customization for profiles, such that Readarr will always download the copy you want
* Fully configurable book renaming
* SABnzbd, NZBGet, QBittorrent, Deluge, rTorrent, Transmission, uTorrent, and other download clients are supported and integrated
* Full integration with Calibre (add to library, conversion) (Requires Calibre Content Server)

Note that only one type of a given book is supported. If you want both an audiobook and an ebook of a given book, run separate instances; this fork intentionally keeps ebook and audiobook libraries separate.

## Upstream sync

This project shares its core with Sonarr, which is still actively developed. A scheduled workflow ([`sonarr-watch.yml`](.github/workflows/sonarr-watch.yml)) scans Sonarr for changes in shared areas (download clients, indexers, parser, media files, organizer, health checks, datastore, auth, and common infrastructure) and reports candidates on the tracking issue. Anything worth adopting is turned into a regular issue before it is worked on.

## Support and contributing

- Bugs and feature requests: [open an issue](https://github.com/iuliandita/readarr/issues).
- How to build and test: see [CONTRIBUTING.md](CONTRIBUTING.md).

## License

* [GNU GPL v3](http://www.gnu.org/licenses/gpl.html)
* Readarr is a Servarr project; this fork keeps the same license.
