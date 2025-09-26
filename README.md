# syncapp

sync program between 2 paths

## Key Features & Benefits

*   Synchronizes files between two specified directories.
*   Uses C# for efficient and reliable performance.
*   MIT Licensed, allowing for flexible usage and modification.

## Prerequisites & Dependencies

*   [.NET SDK](https://dotnet.microsoft.com/en-us/download) (version compatible with the project - check the project's .csproj file for the target framework)
*   Any code editor or IDE suitable for C# development (e.g., Visual Studio, VS Code with C# extension, Rider).

## Installation & Setup Instructions

1.  **Clone the repository:**

    ```bash
    git clone https://github.com/harelo12/syncapp.git
    cd syncapp
    ```

2.  **Navigate to the project directory:**

    ```bash
    cd ConsoleApp1
    ```

3.  **Restore Dependencies (if necessary):**

    This step might not be explicitly required if using an IDE like Visual Studio that automatically restores packages.  If packages need to be restored via command line:

    ```bash
    dotnet restore
    ```

4.  **Build the project:**

    ```bash
    dotnet build
    ```

5.  **Run the application:**

    ```bash
    dotnet run
    ```

## Usage Examples & API Documentation

This is a command-line application. Example usage involves configuring the source and destination paths.  As no code exists within this README, detailed API documentation cannot be created. You will need to inspect the source code (`ConsoleApp1/Program.cs` or equivalent) to understand specific options and parameters.

**Conceptual example (replace with actual implementation details after reviewing the C# code):**

Assume the program takes source and destination paths as arguments:

```bash
dotnet run --source "/path/to/source/directory" --destination "/path/to/destination/directory"
```

## Configuration Options

Configuration depends on the specific implementation within the C# code. Look for command-line arguments, environment variables, or configuration files used by the application.  Possible options include:

*   **Source Directory:** The path to the directory to sync *from*.
*   **Destination Directory:** The path to the directory to sync *to*.
*   **Sync Mode:** (e.g., one-way, two-way, incremental, full).
*   **Filter Options:** (e.g., exclude certain file types or directories).
*   **Logging Level:** Configure verbosity of logging output.

## Contributing Guidelines

1.  Fork the repository.
2.  Create a new branch for your feature or bug fix.
3.  Make your changes and commit them with descriptive messages.
4.  Test your changes thoroughly.
5.  Submit a pull request to the main branch.

Please follow coding style conventions and provide clear explanations of your contributions.

## License Information

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.  (Note: you might need to create a LICENSE file in the repo)

## Acknowledgments

N/A
