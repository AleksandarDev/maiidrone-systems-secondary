#addin "Cake.Putty"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var framework = Argument("framework", "netcoreapp2.0");

///////////////////////////////////////////////////////////////////////
// ARGUMENTS (WITH DEFAULT PARAMETERS FOR WIN 10 IOT CORE)
///////////////////////////////////////////////////////////////////////
//var runtime = Argument("runtime", "win10-arm");
//var destinationIp = Argument("destinationPi", "<<the-pi-ip-address>>");
//var destinationDirectory = Argument("destinationDirectory", @"<<the-deployment-folder>>");

///////////////////////////////////////////////////////////////////////
// ARGUMENTS (WITH DEFAULT PARAMETERS FOR LINUX (Ubuntu 16.04, Raspbian Jessie, etc)
///////////////////////////////////////////////////////////////////////
var runtime = Argument("runtime", "linux-arm");
var destinationIp = Argument("destinationPi", "192.168.0.8");
var destinationDirectory = Argument("destinationDirectory", @"rpidotnet");
var username = Argument("username", "pi");
var executableName = Argument("executableName", "rpidotnet");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var binaryDir = Directory("./bin");
var objectDir = Directory("./obj");
var publishDir = Directory("./publish");
var projectFile = "./" + executableName + ".csproj";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(binaryDir);
        CleanDirectory(objectDir);
        CleanDirectory(publishDir);
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreRestore(projectFile);
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        var settings = new DotNetCoreBuildSettings
        {
            Framework = framework,
            Configuration = configuration,
            OutputDirectory = "./bin/"
        };

        DotNetCoreBuild(projectFile, settings);
    });

Task("Publish")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var settings = new DotNetCorePublishSettings
        {
            Framework = framework,
            Configuration = configuration,
            OutputDirectory = "./publish/",
            Runtime = runtime
        };
                    
        DotNetCorePublish(projectFile, settings);
    });

Task("Deploy")
    .IsDependentOn("Publish")
    .Does(() =>
    {
        var files = GetFiles("./publish/*");
        
        if(runtime.StartsWith("win")) 
        {
            var destination = @"\\" + destinationIp + @"\" + destinationDirectory;
            CopyFiles(files, destination, true);
        }
        else
        {
            var destination = destinationIp + ":" + destinationDirectory;
            var fileArray = files.Select(m => @"""" + m.ToString() + @"""").ToArray();
            Pscp(fileArray, 
                destination, 
                new PscpSettings { 
                    SshVersion = SshVersion.V2, 
                    User = username ,
                    Password = "raspberry",
                    Batch = true                                                
            });

            var plinkCommand = "chmod u+x,o+x " + destinationDirectory + "/" + executableName;
            Plink(username + "@" + destination, 
                plinkCommand,
                new PlinkSettings{ Password="raspberry" });
        }
    });

Task("Default")
    .IsDependentOn("Deploy");

RunTarget(target);