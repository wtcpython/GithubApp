# GithubApp

一个简易的Github App，基于 Windows App SDK 和 Github API 实现

## 注意事项

由于 Github API 限制，如果不提供 token，将会有 API 访问限制，因此每次软件启动时，都会要求用户输入 token。

## 亮点

针对国内用户，由于 Github Website 不方便访问，经常无法裸连，但是 Github API 是可以正常访问的，因此本软件通过调用 API 的方式，实现了部分 Github Web 样式的界面

## 安装该软件的最低条件

1. 最新版本的 [Windows App SDK](https://learn.microsoft.com/zh-cn/windows/apps/windows-app-sdk/downloads)
2. 至少 Windows 11 21H2 (Build 22000)，后续将考虑添加 Win10 支持

## 如何从源代码构建

1. [Visual Studio 2022](https://visualstudio.microsoft.com/zh-hans/vs/)
2. 确保安装 `.NET 桌面开发`， `通用 Windows 平台开发` 等组件
3. [.NET Core 8.0](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)
4. 一些相关的 `NuGet` 包
