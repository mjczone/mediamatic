# Contributing to MediaMatic

Thank you for your interest in contributing to MediaMatic! 🎉

## 🚧 Current Project Status

**This library is currently in active development and early versions (0.x.x).** While we appreciate community interest, please be aware that:

- **Breaking changes are expected** as we refine the API and architecture
- **Major refactoring efforts** are ongoing to improve code quality and consistency
- **Core functionality** is still being solidified across storage providers
- **Public API** may change significantly between releases

## 💡 How You Can Help

While we're not actively seeking large code contributions at this time, **your ideas and feedback are very welcome!**

### Ways to Contribute

✅ **Report Issues**
- Bug reports with clear reproduction steps
- Performance issues or unexpected behavior
- Documentation errors or unclear examples
- Provider-specific compatibility problems

✅ **Share Ideas**
- Feature suggestions for future development
- Use case scenarios we should consider
- API design feedback and improvements
- Storage provider requests

✅ **Improve Documentation**
- Fix typos, grammar, or formatting issues
- Add code examples or clarify existing ones
- Suggest better explanations for complex concepts
- Report missing or outdated information

## 🐛 Reporting Issues

When reporting bugs, please include:

1. **Library version** you're using
2. **Storage provider** and configuration (S3, GCP, Local, SFTP, etc.)
3. **Minimal reproduction code** that demonstrates the issue
4. **Expected vs actual behavior**
5. **Error messages** or stack traces if applicable
6. **Operating system** and .NET version

## 💭 Suggesting Features

For feature requests, please provide:

1. **Clear description** of the proposed feature
2. **Use case** - why is this feature needed?
3. **Example usage** - how would developers use it?
4. **Provider compatibility** - should it work across all storage providers?

## 🔮 Future Contribution Opportunities

As the library matures and reaches **version 1.0**, we plan to welcome:

- Additional storage provider implementations
- Performance optimizations and benchmarks
- Extended test coverage for edge cases
- Advanced media processing features
- Community-driven examples and tutorials

## 📝 Code Contributions (Limited)

If you'd like to contribute code during this early phase:

1. **Open an issue first** to discuss the change
2. **Keep changes small and focused** - large PRs may be difficult to review
3. **Follow existing code patterns** and StyleCop rules
4. **Include tests** for any new functionality
5. **Update documentation** if needed

**Note:** We may not be able to accept all code contributions during this phase due to ongoing architectural changes.

## 🏗️ Development Setup

If you want to explore the codebase:

```bash
# Clone the repository
git clone https://github.com/mjczone/mediamatic.git
cd MJCZone.MediaMatic

# Build the solution
dotnet build

# Run tests (requires Docker for storage containers)
dotnet test
```

## 📞 Getting Help

- **GitHub Issues** - For bugs, features, and questions
- **GitHub Discussions** - For general questions and community chat
- **Documentation** - Check [our docs](https://mjczone.github.io/mediamatic/) first

## 🤝 Code of Conduct

This project follows a simple code of conduct:

- **Be respectful** and constructive in all interactions
- **Stay focused** on technical discussions
- **Help others** learn and grow
- **Assume positive intent** in communications

## 🎯 Roadmap

We're working toward a stable **1.0 release** with:

- 🔄 Core media storage abstraction
- 🔄 Image processing and optimization
- 🔄 Video processing and thumbnail generation
- 🔄 Browser-aware format negotiation
- 🔄 Comprehensive test coverage
- 🔄 API stabilization and documentation completion
- 📋 Community feedback integration

## 📄 License

By contributing to this project, you agree that your contributions will be licensed under the same [GNU Lesser General Public License v3.0 or later (LGPL-3.0-or-later)](LICENSE) that covers the project.

---

**Thank you for your interest in MediaMatic!**

Even though we're limiting code contributions during this development phase, your feedback, ideas, and issue reports are invaluable in helping us build a better library. 🚀
