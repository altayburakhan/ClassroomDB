# Create necessary directories
New-Item -ItemType Directory -Force -Path "wwwroot\lib\mantis\css"
New-Item -ItemType Directory -Force -Path "wwwroot\lib\mantis\js"
New-Item -ItemType Directory -Force -Path "wwwroot\lib\mantis\fonts"
New-Item -ItemType Directory -Force -Path "wwwroot\images\users"

# Copy CSS files
Copy-Item -Path "..\Bootstrap\Mantis-Bootstrap-1.0.0\dist\assets\css\*.css" -Destination "wwwroot\lib\mantis\css" -Force
Copy-Item -Path "..\Bootstrap\Mantis-Bootstrap-1.0.0\dist\assets\css\plugins" -Destination "wwwroot\lib\mantis\css" -Recurse -Force

# Copy JS files
Copy-Item -Path "..\Bootstrap\Mantis-Bootstrap-1.0.0\dist\assets\js\*.js" -Destination "wwwroot\lib\mantis\js" -Force
Copy-Item -Path "..\Bootstrap\Mantis-Bootstrap-1.0.0\dist\assets\js\plugins" -Destination "wwwroot\lib\mantis\js" -Recurse -Force

# Copy images
Copy-Item -Path "..\Bootstrap\Mantis-Bootstrap-1.0.0\dist\assets\images\*" -Destination "wwwroot\images" -Recurse -Force

# Copy fonts
Copy-Item -Path "..\Bootstrap\Mantis-Bootstrap-1.0.0\dist\assets\fonts\*" -Destination "wwwroot\lib\mantis\fonts" -Recurse -Force 