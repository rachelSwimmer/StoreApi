Option 2 (Best Practice): .NET User Secrets 

This is the correct way to keep secrets with appsettings keys but outside Git. 

Step 1: Initialize user secrets 

dotnet user-secrets init 
 

This creates: 

<PropertyGroup> 
 <UserSecretsId>...</UserSecretsId> 
</PropertyGroup> 
 

in the .csproj (safe to commit). 

 

Step 2: Set the secret 

dotnet user-secrets set "ConnectionStrings:Default" "Server=...;Password=..." 
 

✔ Stored outside repo 
✔ Automatically loaded in Development 
✔ Uses same keys as appsettings.json 
