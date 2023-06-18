<br/>
<p align="center">
  <a href="https://github.com/Moonlight-Panel/Moonlight">
    <img src="https://my.endelon-hosting.de/api/moonlight/resources/images/logo.svg" alt="Logo" width="80" height="80">
  </a>

  <h3 align="center">Moonlight</h3>

  <p align="center">
    The next generation hosting panel
    <br/>
    <br/>
    <a href="https://github.com/Moonlight-Panel/Moonlight/issues">Report Bug</a>
    .
    <a href="https://github.com/Moonlight-Panel/Moonlight/issues">Request Feature</a>
  </p>
</p>

![Downloads](https://img.shields.io/github/downloads/Moonlight-Panel/Moonlight/total) ![Contributors](https://img.shields.io/github/contributors/Moonlight-Panel/Moonlight?color=dark-green) ![Stargazers](https://img.shields.io/github/stars/Moonlight-Panel/Moonlight?style=social) ![Issues](https://img.shields.io/github/issues/Moonlight-Panel/Moonlight) 

## About The Project

![Screen Shot](https://media.discordapp.net/attachments/1059911407170228234/1119793539732217876/image.png?width=1340&height=671)

Moonlight is a new free and open source alternative to pterodactyl allowing users to create their own hosting platform and host all sorts of gameservers in docker containers. With a simple migration from pterodactyl to moonlight (migrator will be open source soon) you can easily switch to moonlight and use its features like a server manager, cleanup system and automatic version switcher, just to name a few.

Moonlight's core features are

* Hosting game servers using wings + docker
* Creating and managing webspaces using the cloudpanel based web hosting solution
* Adding your domains as shared domains and provide subdomains for users with them
* Live support chat
* Subscription system (sellpass integration wip)
* Statistics
* and many more

This project is currently in beta

## Built With



* [Bootstrap 5](https://getbootstrap.com/)
* [Blazor Server Side](https://learn.microsoft.com/de-de/aspnet/core/blazor/hosting-models?view=aspnetcore-7.0)
* [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

## Getting Started


### Prerequisites

* Linux based operating system
* Docker
* MySQL Database
* A domain (optional)

### Installation

A full guide how to install moonlight can be found here:
[https://docs.moonlightpanel.xyz/installing-moonlight](https://docs.moonlightpanel.xyz/installing-moonlight)

Quick installers/updaters:

Moonlight:
`curl https://install.moonlightpanel.xyz/moonlight | bash`

Daemon (not wings):
`curl https://install.moonlightpanel.xyz/daemon| bash`

## Roadmap

The roudmap can be found here:
[https://github.com/orgs/Moonlight-Panel/projects/1](https://github.com/orgs/Moonlight-Panel/projects/1)

## Contributing

* If you have suggestions for adding or removing projects, feel free to open an issue to discuss it.
* Please make sure you check your spelling and grammar.
* Create individual PR for each suggestion.

### Creating A Pull Request

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

Distributed under the Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International Public License. See [LICENSE](https://github.com/Moonlight-Panel/Moonlight/blob/main/LICENSE.md) for more information.

## Authors

* **Marcel Baumgartner** - *Endelon Hosting* - [Marcel Baumgartner](https://github.com/Marcel-Baumgartner) - *Moonlights core system & frontend*
* **Daniel Balk** - *Endelon Hosting* - [Daniel Balk](https://github.com/Daniel-Balk) - *Notification system & frontend*
* **Spielepapagei** - *Endelon Hosting* - [Spielepapagei](https://github.com/Spielepapagei) - *Discord Bot & support tickets*
* **Dannyx** - *None* - [Dannyx](https://github.com/Dannyx1604) - *Grammer check and translations*
