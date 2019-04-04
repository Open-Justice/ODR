ko.bindingHandlers.href = {
    update: function (element, valueAccessor) {
        ko.bindingHandlers.attr.update(element, function () {
            return { href: valueAccessor() }
        });
    }
};

ko.bindingHandlers.name = {
    update: function (element, valueAccessor) {
        ko.bindingHandlers.attr.update(element, function () {
            return { name: valueAccessor() }
        });
    }
};

ko.bindingHandlers.src = {
    update: function (element, valueAccessor) {
        ko.bindingHandlers.attr.update(element, function () {
            return { src: valueAccessor() }
        });
    }
};


ko.bindingHandlers.for = {
    update: function (element, valueAccessor) {
        ko.bindingHandlers.attr.update(element, function () {
            return { 'for': valueAccessor() }
        });
    }
};

ko.bindingHandlers.id = {
    update: function (element, valueAccessor) {
        ko.bindingHandlers.attr.update(element, function () {
            return { id: valueAccessor() }
        });
    }
};


ko.bindingHandlers.dateString = {
    init: function (element, valueAccessor) {
        //attach an event handler to our dom element to handle user input
        element.onchange = function () {
            var value = valueAccessor();//get our observable
            //set our observable to the parsed date from the input
            value(moment(element.value, 'L').toDate());
        };
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var value = valueAccessor();
        var valueUnwrapped = ko.utils.unwrapObservable(value);
        if (valueUnwrapped) {
            element.value = moment(valueUnwrapped).format('L');
        }
    }
};

ko.observableArray.fn.pushAll = function (valuesToPush) {
    var underlyingArray = this();
    this.valueWillMutate();
    ko.utils.arrayPushAll(underlyingArray, valuesToPush);
    this.valueHasMutated();
    return this;  //optional
};