# Rules for Architecting a World-Class AI Agent for Cursor Coding Editor (Unity Development)

## 1. Scope and Requirements
- **Purpose:** The AI agent must assist Unity developers with code generation, refactoring, debugging, and optimization within the Cursor coding editor.
- **Integration:** Seamlessly integrate with Cursor’s interface and Unity’s ecosystem (e.g., MonoBehaviour scripts, Unity APIs).
- **Interaction:** Offer real-time, context-aware suggestions, code completions, and feedback based on the Unity project context.

## 2. Architectural Guidelines
- **Modular Design:** Divide the system into independent modules (e.g., Code Analyzer, Suggestion Engine) for maintainability and scalability.
- **Service-Oriented Approach:** Use loosely coupled services for core functionalities to enable easy updates and replacements.
- **Event-Driven System:** Implement event listeners and handlers for real-time responsiveness (e.g., reacting to code changes or user actions).

## 3. Design Patterns
- **Singleton:** Use for managing single-instance components like a configuration or session manager.
- **Factory:** Dynamically create suggestion or tool instances based on context (e.g., generating Unity-specific code snippets).
- **Observer:** Enable components to react to changes (e.g., code updates triggering suggestion refreshes) without tight coupling.
- **Strategy:** Allow runtime swapping of algorithms (e.g., different optimization strategies for Unity performance).
- **Decorator:** Enhance functionality dynamically (e.g., adding debug logging to suggestions).

## 4. OOP Principles
- **Encapsulation:** Protect internal data and expose only necessary APIs/interfaces.
- **Inheritance:** Reuse code via base classes (e.g., a generic Unity script analyzer extended for specific use cases).
- **Polymorphism:** Enable flexible method calls across related classes (e.g., treating different suggestion types uniformly).
- **Abstraction:** Define interfaces/abstract classes for core components (e.g., `ICodeAnalyzer`, `ISuggestionEngine`).

## 5. Core Components
- **Code Analyzer:** Parse Unity scripts (C#), project structure, and assets to understand context.
- **Suggestion Engine:** Generate Unity-specific code, refactorings, and optimizations based on analysis.
- **Context Manager:** Track the coding session state (e.g., current script, user intent, Unity scene context).
- **UI Handler:** Integrate with Cursor to display suggestions, errors, and feedback.
- **Learning Module (Optional):** Use machine learning to refine suggestions based on user behavior.

## 6. Data Flow and Communication
- **APIs:** Define clear, well-documented APIs for inter-component communication.
- **Asynchronous Processing:** Use message queues for tasks like analyzing large Unity projects.
- **Caching:** Store frequently used data (e.g., Unity API references) to improve performance.

## 7. Testing and Validation
- **Unit Tests:** Ensure each component works independently (e.g., testing the Suggestion Engine’s output).
- **Integration Tests:** Verify component interactions (e.g., Code Analyzer feeding Suggestion Engine).
- **User Testing:** Collect feedback from Unity developers to refine functionality.

## 8. Scalability and Performance
- **Load Distribution:** Support multi-threading or distributed processing for large projects.
- **Optimization:** Profile and optimize for minimal impact on Unity/Cursor performance.
- **Resource Efficiency:** Manage memory and CPU usage effectively.

## 9. Security and Privacy
- **Encryption:** Protect user code and project data.
- **Access Control:** Restrict AI agent capabilities to authorized actions.
- **Compliance:** Follow Unity’s terms and data protection laws.

## 10. Documentation and Support
- **Developer Docs:** Detail the architecture and APIs for future extensions.
- **User Guides:** Provide tutorials for Unity developers using the AI agent in Cursor.
- **Support:** Offer channels for reporting bugs or seeking assistance.

## Example Usage in Codebase
Below is a C# snippet illustrating some of these principles in a Unity context:

```csharp
// Abstract base class for code analysis
public abstract class CodeAnalyzer
{
    public abstract void Analyze(string scriptContent);
}

// Concrete analyzer for Unity scripts
public class UnityCodeAnalyzer : CodeAnalyzer
{
    public override void Analyze(string scriptContent)
    {
        // Parse MonoBehaviour-specific code
        Console.WriteLine("Analyzing Unity script...");
    }
}

// Suggestion Engine interface
public interface ISuggestionEngine
{
    string GenerateSuggestion(string context);
}

// Strategy-based suggestion engine
public class UnitySuggestionEngine : ISuggestionEngine
{
    private readonly ICodeAnalyzer analyzer;

    public UnitySuggestionEngine(ICodeAnalyzer analyzer)
    {
        this.analyzer = analyzer; // Dependency injection
    }

    public string GenerateSuggestion(string context)
    {
        analyzer.Analyze(context);
        return "Add [SerializeField] to expose this variable in Unity Inspector.";
    }
}

// Singleton for configuration
public class ConfigManager
{
    private static ConfigManager instance;
    private ConfigManager() { }

    public static ConfigManager Instance
    {
        get { return instance ??= new ConfigManager(); }
    }
}
```

## Conclusion
These rules ensure the AI agent is built with a scalable, maintainable, and efficient architecture, leveraging design patterns and OOP principles tailored for Unity development in the Cursor editor.