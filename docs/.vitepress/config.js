import { defineConfig } from "vitepress";
import apiSidebar from "../api-sidebar.json" with { type: "json" };

export default defineConfig({
  title: "MediaMatic",
  description:
    "Intelligent media storage, optimization, and processing for .NET",

  base: "/",
  ignoreDeadLinks: true,

  markdown: { theme: { light: "github-light", dark: "github-dark" } },

  themeConfig: {
    logo: "/favicon.ico",

    nav: [
      { text: "Guide", link: "/guide/getting-started" },
      {
        text: "Resources",
        items: [
          { text: ".NET API", link: "/api/" },
          { text: "REST API", link: "/api-browser/" },
        ],
      },
      {
        text: "GitHub",
        link: "https://github.com/mjczone/mediamatic",
      },
    ],

    sidebar: {
      "/guide/": [
        {
          text: "🛠️ Library Usage",
          collapsed: false,
          items: [
            { text: "Getting Started", link: "/guide/getting-started" },
            { text: "Installation", link: "/guide/installation" },
            { text: "Configuration", link: "/guide/configuration" },
            { text: "Storage Providers", link: "/guide/storage-providers" },
          ],
        },
        {
          text: "🖼️ Media Processing",
          collapsed: false,
          items: [
            { text: "Image Processing", link: "/guide/image-processing" },
            { text: "Video Processing", link: "/guide/video-processing" },
            { text: "Metadata Extraction", link: "/guide/metadata-extraction" },
            { text: "Testing", link: "/guide/testing" },
          ],
        },
        {
          text: "🌐 Web API Integration",
          collapsed: true,
          items: [
            { text: "ASP.NET Core", link: "/guide/aspnetcore-integration" },
            { text: "Browser Detection", link: "/guide/browser-detection" },
          ],
        },
        {
          text: "📚 Common Resources",
          collapsed: true,
          items: [
            { text: "Credits", link: "/guide/credits" },
            { text: "Roadmap", link: "/guide/roadmap" },
            { text: "License", link: "/guide/license" },
          ],
        },
      ],
      ...apiSidebar,
    },

    socialLinks: [
      {
        icon: "github",
        link: "https://github.com/mjczone/mediamatic",
      },
    ],

    search: {
      provider: "local",
    },
  },
});
