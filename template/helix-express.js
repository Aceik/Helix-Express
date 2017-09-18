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

var config;
if (fs.existsSync('../gulp-config.js.user')) {
    config = require("../gulp-config.js.user")();
}
else {
    config = require("../gulp-config.js")()
}

module.exports.config = config;

gulp.task("express", function (callback) {
    config.runCleanBuilds = true;
    return runSequence(
        "01-Copy-Sitecore-License",
        "02-Nuget-Restore",
        "Express-03-Publish-All-Projects",
        "04-Apply-Xml-Transform",
        "05-Sync-Unicorn",
        "06-Deploy-Transforms",
        "Publish-All-Configs",
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
    return gulp.src([location + "*.Express.csproj"])
        .pipe(foreach(function (stream, file) {
            return publishStream(stream, dest);
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
    return publishExpressProjects(".");
});

