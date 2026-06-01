> [!NOTE]
> This repository is a maintenance fork for people who still depend on Readarr while the ecosystem waits for a real replacement to emerge. The intent is to keep existing installs usable and safer through security updates, dependency/runtime repair, packaging fixes, and selected breaking bug fixes; it is not a revival of Readarr as a feature-driven project, and new end-user features are out of scope by default. The container image built here keeps a LinuxServer-compatible runtime layout for existing homelab deployments.

Two things worth knowing about how this fork runs. The container is built on the [LinuxServer.io base image](https://github.com/linuxserver/docker-baseimage-alpine) and keeps its s6/PUID-PGID layout, so it drops into existing homelab setups ([LinuxServer docs](https://docs.linuxserver.io/)). For metadata, the original Readarr server is gone, so this fork defaults to the community [rreading-glasses](https://github.com/blampe/rreading-glasses) server (see the "Metadata provider" notes below to read more or self-host).

## Metadata provider

The original Readarr metadata server (`api.bookinfo.club`) is gone (the domain no longer resolves), which left a clean install unable to search, add, or refresh books. To keep the app usable out of the box, this fork defaults the metadata source to the hosted [rreading-glasses](<https://github.com/blampe/rreading-glasses>) public instance at `https://api.bookinfo.pro` (Goodreads-backed).

- **Third party, unsupported, use at your own risk.** It is a free community service run by others; this fork is not affiliated with it and cannot support it. Expect rate limits and possible downtime, and review its terms before relying on it.
- **Overridable.** Change it under **Settings -> Development -> Metadata Source**. For Hardcover-backed metadata, use `https://hardcover.bookinfo.pro`. Clear the field to fall back to the (now dead) built-in default.
- **Self-hosting (recommended for reliability).** Run your own with the `blampe/rreading-glasses:latest` image and point the override at it, e.g. `http://rreading-glasses:8788`.
- **Caveat:** rreading-glasses formats some titles differently from the old server. Existing libraries may need a re-import/refresh of works with long subtitles.

# Announcement: Retirement of Readarr

We would like to announce that the [Readarr project](<https://github.com/Readarr/Readarr>) has been retired. This difficult decision was made due to a combination of factors: the project's metadata has become unusable, we no longer have the time to remake or repair it, and the community effort to transition to using Open Library as the source has stalled without much progress.

Third-party metadata mirrors exist, but as we're not involved with them at all, we cannot provide support for them. Use of them is entirely at your own risk. The most popular mirror appears to be [rreading-glasses](<https://github.com/blampe/rreading-glasses>).

Without anyone to take over Readarr development, we expect it to wither away, so we still encourage you to seek alternatives to Readarr.

## Key Points:
- **Effective Immediately**: The retirement takes effect immediately. Please stay tuned for any possible further communications.
- **Support Window**: We will provide support during a brief transition period to help with troubleshooting non metadata related issues.
- **Alternative Solutions**: Users are encouraged to explore and adopt any other possible solutions as alternatives to Readarr.
- **Opportunities for Revival**: We are open to someone taking over and revitalizing the project. If you are interested, please get in touch.
- **Gratitude**: We extend our deepest gratitude to all the contributors and community members who supported Readarr over the years.

Thank you for being part of the Readarr journey. For any inquiries or assistance during this transition, please contact our team.

Sincerely,  
The Servarr Team

# Readarr

[![Build Status](https://dev.azure.com/Readarr/Readarr/_apis/build/status/Readarr.Readarr?branchName=develop)](https://dev.azure.com/Readarr/Readarr/_build/latest?definitionId=1&branchName=develop)
[![Translated](https://translate.servarr.com/widgets/servarr/-/readarr/svg-badge.svg)](https://translate.servarr.com/engage/readarr/?utm_source=widget)
[![Docker Pulls](https://img.shields.io/docker/pulls/hotio/readarr)](https://wiki.servarr.com/readarr/installation#docker)
[![Donors on Open Collective](https://opencollective.com/Readarr/backers/badge.svg)](#backers)
[![Sponsors on Open Collective](https://opencollective.com/Readarr/sponsors/badge.svg)](#sponsors)
[![Mega Sponsors on Open Collective](https://opencollective.com/Readarr/megasponsors/badge.svg)](#mega-sponsors)

### Readarr is currently in beta testing and is generally still in a work in progress. Features may be broken, incomplete, or cause spontaneous combustion

Readarr is an ebook and audiobook collection manager for Usenet and BitTorrent users. It can monitor multiple RSS feeds for new books from your favorite authors and will grab, sort, and rename them.
Note that only one type of a given book is supported. If you want both an audiobook and ebook of a given book you will need multiple instances.

## Major Features Include

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
* And a beautiful UI

## Support

[![Wiki](https://img.shields.io/badge/servarr-wiki-181717.svg?maxAge=60)](https://wiki.servarr.com/readarr)
[![Discord](https://img.shields.io/badge/discord-chat-7289DA.svg?maxAge=60)](https://readarr.com/discord)

Note: GitHub Issues are for Bugs and Feature Requests Only

[![GitHub - Bugs and Feature Requests Only](https://img.shields.io/badge/github-issues-red.svg?maxAge=60)](https://github.com/Readarr/Readarr/issues)

## Contributors & Developers

[API Documentation](https://readarr.com/docs/api/)

This project exists thanks to all the people who contribute.
- [Contribute (GitHub)](CONTRIBUTING.md)
- [Contribution (Wiki Article)](https://wiki.servarr.com/readarr/contributing)

[![Contributors List](https://opencollective.com/Readarr/contributors.svg?width=890&button=false)](https://github.com/Readarr/Readarr/graphs/contributors)

## Backers

Thank you to all our backers! 🙏 [Become a backer](https://opencollective.com/Readarr#backer)

[![Backers List](https://opencollective.com/Readarr/backers.svg?width=890)](https://opencollective.com/Readarr#backer)

## Sponsors

Support this project by becoming a sponsor. Your logo will show up here with a link to your website. [Become a sponsor](https://opencollective.com/readarr#sponsor)

[![Sponsors List](https://opencollective.com/Readarr/sponsors.svg?width=890)](https://opencollective.com/readarr#sponsor)

## Mega Sponsors

[![Mega Sponsors List](https://opencollective.com/Readarr/tiers/mega-sponsor.svg?width=890)](https://opencollective.com/readarr#mega-sponsor)

## DigitalOcean

This project is also supported by DigitalOcean
<p>
  <a href="https://www.digitalocean.com/">
    <img src="https://opensource.nyc3.cdn.digitaloceanspaces.com/attribution/assets/SVG/DO_Logo_horizontal_blue.svg" width="201px">
  </a>
</p>

### License

* [GNU GPL v3](http://www.gnu.org/licenses/gpl.html)
* Copyright 2010-2022
