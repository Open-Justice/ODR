var gulp = require('gulp');
var less = require('gulp-less');
var path = require('path');
var LessPluginCleanCSS = require('less-plugin-clean-css');
var cleancss = new LessPluginCleanCSS({ advanced: true });


gulp.task('less', function () {
    gulp.src('./Content/main.less')
      .pipe(less({
          plugins: [cleancss]
      }))
      .pipe(gulp.dest('./Content'));
});

gulp.task('pricing-less', function () {
    gulp.src('./Content/less/pricing.less')
    .pipe(less({
        plugins: [cleancss]
    }))
    .pipe(gulp.dest('./Content/css'));
})


gulp.task('watch', function () {
    gulp.watch('./Content/less/*.less', ['less', 'pricing-less']);  // Watch all the .less files, then run the less task
});

gulp.task('default', ['less', 'pricing-less', 'watch']); // Default will run the 'entry' watch task

//var mjml = require('gulp-mjml')
//var mjmlEngine = require('mjml')

//gulp.task('build-mjml', function () {
//    return gulp.src('./Emails/mjml/welcome2.mjml')
//    .pipe(mjml())
//    .pipe(gulp.dest('./Emails'))
//})
