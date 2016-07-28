"use strict";

var gulp = require('gulp');
var runSequence = require('run-sequence');
var conventionalChangelog = require('gulp-conventional-changelog');
var bump = require('gulp-bump');
var gutil = require('gulp-util');
var insert = require('gulp-insert');
var trim = require('gulp-trim');
var xmlpoke = require('gulp-xmlpoke');
var replace = require('gulp-replace');
var assemblyInfo = require('gulp-dotnet-assembly-info');
var git = require('gulp-git');
var fs = require('fs');
var es = require('event-stream');

function getVersion(format) {
    var version = JSON.parse(fs.readFileSync('./package.json', 'utf8')).version;
    if (format) {
        var match = /((?:0|[1-9][0-9]*)\.(?:0|[1-9][0-9]*)\.(?:0|[1-9][0-9]*))(?:-([\da-z\-]+)\.(0|[1-9][0-9]?))?/i.exec(version);
        if (format === 'nuget') {
            if (match[2]) {
                version = match[1] + '-' + match[2] + ('0' + match[3]).slice(-2);
            }
        } else if (format === 'no-pr') {
            version = match[1];
        }
    }
    return version;
}

gulp.task('bump-version', function () {
    var type = 'patch';
    var types = ['major', 'minor', 'patch', 'prerelease'];
    for (var i = 0; i < 4; i++) {
        if (process.argv.indexOf('--' + types[i]) > -1) {
            type = types[i];
            break;
        }
    }

    return gulp.src(['./package.json'])
      .pipe(bump({ type: type, preid: 'prerelease' }).on('error', gutil.log))
      .pipe(gulp.dest('.'));
});

gulp.task('changelog', function () {
    return gulp.src('CHANGELOG.md', { buffer: false })
        .pipe(conventionalChangelog({ preset: 'angular' }))
        .pipe(gulp.dest('.'));
});

gulp.task('releasenotes', function () {
    return gulp.src('RELEASENOTES.md')
        .pipe(insert.transform(function () { return ''; })) 
        .pipe(conventionalChangelog({ preset: 'angular' }, {}, {}, {}, { headerPartial: '' }))
        .pipe(trim())
        .pipe(gulp.dest('.'));
});

gulp.task('update-nuspec', function () {
    var version = getVersion('nuget');
    return gulp.src('NuGet.nuspec')
        .pipe(xmlpoke({
            replacements: [
                { xpath: '/x:package/x:metadata/x:version', namespaces: { "x": "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd" }, value: version }
            ]
        }))
        .pipe(gulp.dest('./'));
});

gulp.task('update-setup', function () {
    var version = getVersion('no-pr');
    return gulp.src('./SMLogging.Setup/Product.wxs')
        .pipe(xmlpoke({
            replacements: [
                { xpath: '/x:Wix/x:Product/@Version', namespaces: { "x": "http://schemas.microsoft.com/wix/2006/wi" }, value: version }
            ]
        }))
        .pipe(gulp.dest('./SMLogging.Setup/'));
});

gulp.task('update-assemblyinfo', function () {
    var version = getVersion('no-pr');
    return gulp.src('**/AssemblyInfo.cs')
        .pipe(assemblyInfo({
            version: version + '.0',
            fileVersion: version + '.0'
        }))
        .pipe(gulp.dest('.'));
});

gulp.task('update-appveyor', function () {
    var version = getVersion('no-pr');
    return gulp.src('./appveyor.yml')
        .pipe(replace(/VERSION_PREFIX:.*/, 'VERSION_PREFIX: ' + version))
        .pipe(gulp.dest('.'));
});

gulp.task('commit-changes', function () {
    return gulp.src('.')
      .pipe(git.add())
      .pipe(git.commit('chore(release): bump version, update changelog'));
});

gulp.task('push-changes', function (cb) {
    return git.push('origin', 'master', cb);
});

gulp.task('create-new-tag', function (cb) {
    var version = getVersion();
    return git.tag(version, 'create tag for version: ' + version, function (error) {
        if (error) {
            return cb(error);
        }
        return git.push('origin', 'master', { args: '--tags' }, cb);
    });
});

gulp.task('release', function (callback) {
    runSequence(
      'bump-version',
      'changelog',
      'releasenotes',
      'update-nuspec',
      'update-setup',
      'update-assemblyinfo',
      'update-appveyor',
      'commit-changes',
      'push-changes',
      'create-new-tag',
      function (error) {
          if (error) {
              console.log(error.message);
          } else {
              console.log('Release successful');
          }
          callback(error);
      });
});