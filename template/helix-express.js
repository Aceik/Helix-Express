var gulp = require("gulp");
var msbuild = require("gulp-msbuild");
var debug = require("gulp-debug");
var foreach = require("gulp-foreach");
var rename = require("gulp-rename");
var newer = require("gulp-newer");
var util = require("gulp-util");
var runSequence = require("run-sequence");
var nugetRestore = require('gulp-nuget-restore');
var fs = require('fs');
var yargs = require("yargs").argv;
var unicorn = require("./unicorn.js");
var habitat = require("./habitat.js");
var helix = require("./helix.js");
var del = require('del');
var replace = require('gulp-string-replace');

var exec = require('child_process').exec;

var config;
if (fs.existsSync('../gulp-config.js.user')) {
    config = require("../gulp-config.js.user")();
}
else {
    config = require("../gulp-config.js")()
}

module.exports.config = config;

gulp.task("express-setup", function (callback) {
    config.runCleanBuilds = true;
    return runSequence(
        "Express-Patch-Unicorn-Location",
        "Express-Patch-Unicorn-Files",
		"clean:express:old",
		"express-convert-solution"
        callback);
});

gulp.task('express-convert-solution', function (cb) {
  var batchLocation = __dirname + '\\helix-express.bat';
  exec(batchLocation, function (err, stdout, stderr) {
	console.log(stdout);
    console.log(stderr);
    cb(err);
  });
  
})


gulp.task("express", function (callback) {
    config.runCleanBuilds = true;
    return runSequence(
        "01-Copy-Sitecore-License",
        "02-Nuget-Restore",
        "Express-03-Publish-All-Projects",
        "04-Apply-Xml-Transform",
		"clean:configs",
        "Express-Publish-All-Configs",
        "05-Sync-Unicorn",
        "06-Deploy-Transforms",
        callback);
});

gulp.task("Express-03-Publish-All-Projects", function (callback) {
    return runSequence(
        "Express-Build-Solution",
        "Express-Publish-All-Projects", callback);
});

var publishExpressProjects = function (location, dest) {
    dest = dest || config.websiteRoot;
    console.log("publish to " + dest + " folder");
	console.log("location to " + location + " folder");
    return gulp.src([location + "*.Express.csproj"])
        .pipe(foreach(function (stream, file) {
            return expressPublishStream(stream, dest);
        }));
};

gulp.task("Express-Build-Solution", function () {
    var targets = ["Build"];
    if (config.runCleanBuilds) {
        targets = ["Clean", "Build"];
    }

    var solution = "./" + config.expressSolutionName + ".sln";
    return gulp.src(solution)
        .pipe(msbuild({
            targets: targets,
            configuration: config.buildConfiguration,
            logCommand: false,
            verbosity: config.buildVerbosity,
            stdout: true,
            errorOnFail: true,
            maxcpucount: config.buildMaxCpuCount,
            nodeReuse: false,
            toolsVersion: config.buildToolsVersion,
            properties: {
                Platform: config.buildPlatform
            }
        }));
});

gulp.task("Express-Publish-All-Projects", function () {
    return publishExpressProjects("./");
});

gulp.task("Express-Publish-Assemblies", function () {
    var root = "./src";

    var binFiles = [
        root + "/bin/Sitecore.*.Express.*.{dll,pdb}",
        root + "/**/code/**/bin/Sitecore.*.Express.*.{dll,pdb}"
    ];

    var destination = config.websiteRoot + "/bin/";
    return gulp.src(binFiles, { base: root })
        .pipe(rename({ dirname: "" }))
        .pipe(newer(destination))
        .pipe(debug({ title: "Copying " }))
        .pipe(gulp.dest(destination));
});

var expressPublishStream = function (stream, dest) {
  var targets = ["Build"];

  return stream
    .pipe(debug({ title: "Building project:" }))
    .pipe(msbuild({
      targets: targets,
      configuration: config.buildConfiguration,
      logCommand: false,
      verbosity: config.buildVerbosity,
      stdout: true,
      errorOnFail: true,
      maxcpucount: config.buildMaxCpuCount,
      nodeReuse: false,
      toolsVersion: config.buildToolsVersion,
      properties: {
        Platform: config.publishPlatform,
        DeployOnBuild: "true",
        DeployDefaultTarget: "WebPublish",
        WebPublishMethod: "FileSystem",
        DeleteExistingFiles: "false",
        publishUrl: dest,
        _FindDependencies: "false"
      }
    }));
}

gulp.task("express-pac", function (callback) {
    config.runCleanBuilds = true;
    return runSequence(
		"clean:configs",
        "Express-Publish-All-Configs",
        callback);
});

gulp.task("Express-Publish-All-Configs", function () {
  var root = "./src";
  var roots = [root + "/**/App_Config", "!" + root + "/**/obj/**/App_Config"];
  var files = "/**/*.config";
  var destination = config.websiteRoot + "\\App_Config";
  return gulp.src(roots, { base: root }).pipe(
    foreach(function (stream, file) {
      console.log("Publishing from " + file.path);
      gulp.src(file.path + files, { base: file.path })
		
		.pipe(replace(','+  config.companyPrefix +'\.Foundation\.([a-zA-Z])*', ', Sitecore.Foundation.Express'))
		.pipe(replace(', '+  config.companyPrefix +'\.Foundation\.([a-zA-Z])*', ', Sitecore.Foundation.Express'))
		.pipe(replace(','+  config.companyPrefix +'\.Feature\.([a-zA-Z])*', ', Sitecore.Feature.Express'))
		.pipe(replace(', '+  config.companyPrefix +'\.Feature\.([a-zA-Z])*', ', Sitecore.Feature.Express'))
		.pipe(replace(','+  config.companyPrefix +'\.Project\.([a-zA-Z\.])*', ', Sitecore.Project.Express'))
		.pipe(replace(', '+  config.companyPrefix +'\.Project\.([a-zA-Z\.])*', ', Sitecore.Project.Express'))
		.pipe(newer(destination))
        .pipe(debug({ title: "Copying " }))
		
        .pipe(gulp.dest(destination));
      return stream;
    })
  );
});

gulp.task("Express-Patch-Web-Config", function () {
  gulp.src([config.websiteRoot + "/web.config"])
    .pipe(replace(', '+  config.companyPrefix +'\.Foundation\.([a-zA-Z])*', ', Sitecore.Foundation.Express'))
	.pipe(replace(', '+  config.companyPrefix +'\.Feature\.([a-zA-Z])*', ', Sitecore.Feature.Express'))
	.pipe(replace(', '+  config.companyPrefix +'\.Project\.([a-zA-Z\.])*', ', Sitecore.Project.Express'))
    .pipe(gulp.dest(config.websiteRoot + '/'));
});

gulp.task("Express-Patch-Unicorn-Location", function () {
  var root = "./src";
  var roots = [root];
  var files = "/**/z.FitnessFirst.DevSettings.config";
  var destination = "./src-express-unicorn";
  return gulp.src(roots, { base: root }).pipe(
    foreach(function (stream, file) {
      console.log("Publishing from " + file.path);
      gulp.src(file.path + files, { base: file.path })
		
		.pipe(replace('src', 'exp'))

        .pipe(debug({ title: "Copying " }))
		
        .pipe(gulp.dest(file.path));
      return stream;
    })
  );
});

gulp.task("Express-Patch-Unicorn-Files", function () {
  var root = "./src";
  var roots = [root];
  var files = "/**/**/serialization/**/*.yml";
  var destination = "./exp";
  return gulp.src(roots, { base: root }).pipe(
    foreach(function (stream, file) {
      console.log("Publishing from " + file.path);
      gulp.src(file.path + files, { base: file.path })
		
		.pipe(replace(','+  config.companyPrefix +'\.Foundation\.([a-zA-Z])*', ', Sitecore.Foundation.Express'))
		.pipe(replace(', '+  config.companyPrefix +'\.Foundation\.([a-zA-Z])*', ', Sitecore.Foundation.Express'))
		.pipe(replace(','+  config.companyPrefix +'\.Feature\.([a-zA-Z])*', ', Sitecore.Feature.Express'))
		.pipe(replace(', '+  config.companyPrefix +'\.Feature\.([a-zA-Z])*', ', Sitecore.Feature.Express'))
		.pipe(replace(','+  config.companyPrefix +'\.Project\.([a-zA-Z\.])*', ', Sitecore.Project.Express'))
		.pipe(replace(', '+  config.companyPrefix +'\.Project\.([a-zA-Z\.])*', ', Sitecore.Project.Express'))

		.pipe(newer(destination))
        .pipe(debug({ title: "Copying " }))
		
        .pipe(gulp.dest(destination));
      return stream;
    })
  );
});

gulp.task('clean:express:old', function () {
    return cleanUp(config.websiteRoot + "/bin/"+  config.companyPrefix +".*.{dll,pdb}");
});

var cleanUp = function (location) {
    console.log("Cleaning location: " + location);
    return del([
            location
        ],
        { force: true });
};

