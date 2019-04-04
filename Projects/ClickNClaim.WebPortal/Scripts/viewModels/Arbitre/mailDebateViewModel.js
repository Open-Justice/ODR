var ViewModel = null;
function viewModel(data) {
    var self = this;
    self.data = data;
    self.countDown = ko.observable('');
    self.begin = moment().unix();
    self.end = moment(data.CreateDate).add(data.CountDown, 'days').unix();
    self.duration = moment.duration((self.end - self.begin), 'seconds');
    self.countDown(self.duration.days() + 'j ' + self.duration.hours() + ':' + self.duration.minutes() + ':' + self.duration.seconds());

    if (data.CountDown != null && self.duration.asSeconds() > 0) {
        setInterval(function () {
            var begin = moment().unix();
            var end = moment(data.CreateDate).add(data.CountDown, 'days').unix();
            var duration = moment.duration((end - begin), 'seconds');

            self.countDown(duration.days() + 'j ' + duration.hours() + ':' + duration.minutes() + ':' + duration.seconds());
        }, 1000);
    }

    ViewModel = self;
}