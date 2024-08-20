$(document).ready(function () {
    // Load provinces on page load


    $.getJSON('/Regions/GetProvinces', function (data) {
        var province = personProvince;
        $.each(data, function (i, province) {
            $('#province').append($('<option>', {
                value: province,
                text: province
            }));
        });
    });

    // Load cities when a province is selected
    $('#province').click(function () {
        $.getJSON('/Regions/GetProvinces', function (data) {
            $.each(data, function (i, province) {
                $('#province').append($('<option>', {
                    value: province,
                    text: province
                }));
            });
        });
        $('#city').empty();
        $('#area').empty();
        $('#street').empty();
        $('#village').empty();
        if (province) {
            $.getJSON('/Regions/GetCities', { province: province }, function (data) {
                $.each(data, function (i, city) {
                    $('#city').append($('<option>', {
                        value: city,
                        text: city
                    }));
                });
            });
        }
    });


    $('#city').click(function () {
        var city = $(this).val();
        $('#area').empty();
        $('#street').empty();
        $('#village').empty();
        if (city) {
            $.getJSON('/Regions/GetAreas', { city: city }, function (data) {
                $.each(data, function (i, area) {
                    $('#area').append($('<option>', {
                        value: area,
                        text: area
                    }));
                });
            });
        }
    });


    $('#area').click(function () {
        var area = $(this).val();
        $('#street').empty();
        $('#village').empty();
        if (area) {
            $.getJSON('/Regions/GetStreets', { area: area }, function (data) {
                $.each(data, function (i, street) {
                    $('#street').append($('<option>', {
                        value: street,
                        text: street
                    }));
                });
            });
        }
    });

    // Load villages when a street is selected
    $('#street').click(function () {
        var street = $(this).val();
        $('#village').empty();
        if (street) {
            $.getJSON('/Regions/GetVillages', { street: street }, function (data) {
                $.each(data, function (i, village) {
                    $('#village').append($('<option>', {
                        value: village,
                        text: village
                    }));
                });
            });
        }
    });
});
