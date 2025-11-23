import DefaultTheme from "vitepress/theme";
import "./custom.css";
import "./scalar-custom.css";

export default {
  extends: DefaultTheme,
  enhanceApp({ app }) {
    // You can register global components here if needed
  },
};
