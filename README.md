# Helix Express

This project provides the GULP command line tools to flatten your Helix solution down in 3 x layer projects. 

Speed up your deployment time. 

# Express Version

After following the installation instructions [Wiki](../../wiki) and running the gulp command "gulp express-setup" the root of your project will contain a new Visual Studio Solution and 3 projects. 

![](https://i1.wp.com/aceiksolutions.files.wordpress.com/2017/09/sln.png?ssl=1&w=450)
![](https://i1.wp.com/aceiksolutions.files.wordpress.com/2017/09/projects.png?ssl=1&w=450)
![](https://i1.wp.com/aceiksolutions.files.wordpress.com/2017/09/express.png?ssl=1&w=450)

Building in express mode, means only 3 projects have to be compiled.  As opposed to 20+ in the typical helix site. 

# Express Usage

This is not a replacement for the Helix architecture. When you develop with Helix your doing so because the long term benefits of using modules outways throwing all that code into the same project. 

We still recommend you fully maintain your Helix architecture in the project you build. 

That said Helix Express could be used to: 

* On low spec computers where Visual Studio really struggles to load a full helix solution.
* For quicker deployments
* For support and maintance purposes. If your doing lots of bug fixing using Helix Express is may be a quicker way to verify fixes. 
* On a testing machine or for FED developers who need to run the solution. This could save them sometime. 


# Installation

1)  Add this to gulpfile.js in the root folder:
`var express = require("./scripts/express/helix-express");`

2) Copy the whole folder "Express" from the helix-express templates folder over to /scripts/express in the root of you helix project.

3) Edit the config file "Aceik.HelixExpress.Runner.exe.config" in the express folder.

4) from the command prompt in the root solution folder run:
npm install gulp-exec --save-dev
npm install gulp-string-replace --save-dev

5) Add variable `companyPrefix: "MyPrefix"` into gulp-config.js

5) run "npm install" in the root

6) run "gulp express-setup"

-- to remove all the old dlls from running IIS website. 
-- to change the unicorn location
-- to copy the unicorn files to a new location with adjusted namespaces
-- to generate the new Solution file and csproj structure

7) Open and build the solution in Visual Studio

7) run "gulp express"

Cavets 

1)  -- This will only fix config files with namespace matches within Foundation/Feature/Project folders.


