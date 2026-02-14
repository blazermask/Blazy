import os
import json
import subprocess

# List of all files to push
files_to_push = [
    "Blazy.sln",
    "README.md",
    "todo.md",
    "Blazy.Core/Blazy.Core.csproj",
    "Blazy.Core/DTOs/CommentDto.cs",
    "Blazy.Core/DTOs/CreateCommentDto.cs",
    "Blazy.Core/DTOs/CreatePostDto.cs",
    "Blazy.Core/DTOs/LoginDto.cs",
    "Blazy.Core/DTOs/PostDto.cs",
    "Blazy.Core/DTOs/RegisterDto.cs",
    "Blazy.Core/DTOs/UserDto.cs",
    "Blazy.Core/Entities/Comment.cs",
    "Blazy.Core/Entities/Dislike.cs",
    "Blazy.Core/Entities/Like.cs",
    "Blazy.Core/Entities/Post.cs",
    "Blazy.Core/Entities/PostTag.cs",
    "Blazy.Core/Entities/Subscription.cs",
    "Blazy.Core/Entities/Tag.cs",
    "Blazy.Core/Entities/User.cs",
    "Blazy.Core/Entities/UserTag.cs",
    "Blazy.Data/Blazy.Data.csproj",
    "Blazy.Data/BlazyDbContext.cs",
    "Blazy.Data/DataInitializer.cs",
    "Blazy.Repository/Blazy.Repository.csproj",
    "Blazy.Repository/Interfaces/IPostRepository.cs",
    "Blazy.Repository/Interfaces/IRepository.cs",
    "Blazy.Repository/Interfaces/IUserRepository.cs",
    "Blazy.Repository/Repositories/PostRepository.cs",
    "Blazy.Repository/Repositories/Repository.cs",
    "Blazy.Repository/Repositories/UserRepository.cs",
    "Blazy.Services/Blazy.Services.csproj",
    "Blazy.Services/Interfaces/ICommentService.cs",
    "Blazy.Services/Interfaces/IPostService.cs",
    "Blazy.Services/Interfaces/IUserService.cs",
    "Blazy.Services/Services/CommentService.cs",
    "Blazy.Services/Services/PostService.cs",
    "Blazy.Services/Services/UserService.cs",
    "Blazy.Tests/Blazy.Tests.csproj",
    "Blazy.Tests/Services/UserServiceTests.cs",
    "Blazy.Web/Blazy.Web.csproj",
    "Blazy.Web/Controllers/AccountController.cs",
    "Blazy.Web/Controllers/BlogController.cs",
    "Blazy.Web/Controllers/HomeController.cs",
    "Blazy.Web/Controllers/PostController.cs",
    "Blazy.Web/Program.cs",
    "Blazy.Web/Views/Account/Edit.cshtml",
    "Blazy.Web/Views/Account/Login.cshtml",
    "Blazy.Web/Views/Account/Profile.cshtml",
    "Blazy.Web/Views/Account/Register.cshtml",
    "Blazy.Web/Views/Blog/Index.cshtml",
    "Blazy.Web/Views/Home/About.cshtml",
    "Blazy.Web/Views/Home/Index.cshtml",
    "Blazy.Web/Views/Post/Create.cshtml",
    "Blazy.Web/Views/Post/Index.cshtml",
    "Blazy.Web/Views/Shared/_Layout.cshtml",
    "Blazy.Web/Views/_ViewImports.cshtml",
    "Blazy.Web/Views/_ViewStart.cshtml",
    "Blazy.Web/appsettings.json",
    "Blazy.Web/wwwroot/css/site.css"
]

# Split into smaller batches (10 files per batch to avoid command length issues)
batch_size = 10
batches = [files_to_push[i:i + batch_size] for i in range(0, len(files_to_push), batch_size)]

for i, batch in enumerate(batches):
    print(f"Processing batch {i + 1}/{len(batches)}...")
    
    files_array = []
    for file_path in batch:
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
            files_array.append({
                "path": file_path,
                "content": content
            })
        except Exception as e:
            print(f"Error reading {file_path}: {e}")
    
    if files_array:
        # Create JSON for MCP call
        mcp_call = {
            "owner": "blazermask",
            "repo": "Blazy",
            "branch": "main",
            "message": f"Add Blazy project files - Batch {i + 1}/{len(batches)}",
            "files": files_array
        }
        
        # Call MCP tool
        result = subprocess.run(
            ["mcp-tools", "call", "github_push_files", json.dumps(mcp_call)],
            capture_output=True,
            text=True
        )
        
        if result.returncode == 0:
            print(f"Batch {i + 1} pushed successfully!")
        else:
            print(f"Error pushing batch {i + 1}: {result.stderr}")

print("All batches processed!")