

//$(document).ready(function () {
//    var thisProvince;
//    var thisCity;
//    var thisArea;
//    var thisStreet;

//    // Load provinces on page load
//    $.getJSON('/Regions/GetProvinces', function (data) {
//        $.each(data, function (i, province) {
//            $('#province').append($('<option>', {
//                value: province,
//                text: province
//            }));
//        });
//    });

//    // Load cities when a province is selected
//    $('#province').change(function () {
//        var province = $(this).val();
//        $('#city').empty().append('<option value="">Select City</option>');
//        $('#area').empty().append('<option value="">Select Area</option>');
//        $('#street').empty().append('<option value="">Select Street</option>');
//        $('#village').empty().append('<option value="">Select Village</option>');

//        if (province) {
//            $.getJSON('/Regions/GetCities', { province: province }, function (data) {
//                $.each(data, function (i, city) {
//                    $('#city').append($('<option>', {
//                        value: city,
//                        text: city
//                    }));
//                });
//            });
//            thisProvince = province
//        }
//    });

//    // Load areas when a city is selected
//    $('#city').change(function () {
//        var city = $(this).val();
//        $('#area').empty().append('<option value="">Select Area</option>');
//        $('#street').empty().append('<option value="">Select Street</option>');
//        $('#village').empty().append('<option value="">Select Village</option>');

//        if (city) {
//            $.getJSON('/Regions/GetAreas', { city: city, province: thisProvince }, function (data) {
//                $.each(data, function (i, area) {
//                    $('#area').append($('<option>', {
//                        value: area,
//                        text: area
//                    }));
//                });
//            });
//            thisCity = city;
//        }
//    });

//    // Load streets when an area is selected
//    $('#area').change(function () {
//        var area = $(this).val();
//        $('#street').empty().append('<option value="">Select Street</option>');
//        $('#village').empty().append('<option value="">Select Village</option>');

//        if (area) {
//            $.getJSON('/Regions/GetStreets', { city: thisCity, province: thisProvince, area: area }, function (data) {
//                $.each(data, function (i, street) {
//                    $('#street').append($('<option>', {
//                        value: street,
//                        text: street
//                    }));
//                });
//            });
//            thisArea = area;
//        }
//    });

//    // Load villages when a street is selected
//    $('#street').change(function () {
//        var street = $(this).val();
//        $('#village').empty().append('<option value="">Select Village</option>');

//        if (street) {
//            $.getJSON('/Regions/GetVillages', { city: thisCity, province: thisProvince, area: thisArea, street: street }, function (data) {
//                $.each(data, function (i, village) {
//                    $('#village').append($('<option>', {
//                        value: village,
//                        text: village
//                    }));
//                });
//            });
//        }
//    });

//});

$(document).ready(function () {

    function loadProvinces() {
        $.getJSON('/Regions/GetProvinces', function (data) {
            $('#province').empty().append('<option value="">Select Province</option>');
            $.each(data, function (i, province) {
                $('#province').append($('<option>', {
                    value: province,
                    text: province
                }));
            });
        });
    }

    function loadCities(province) {
        $.getJSON('/Regions/GetCities', { province: province }, function (data) {
            $('#city').empty().append('<option value="">Select City</option>');
            $.each(data, function (i, city) {
                $('#city').append($('<option>', {
                    value: city,
                    text: city
                }));
            });
        });
    }

    function loadAreas(city, province) {
        $.getJSON('/Regions/GetAreas', { city: city, province: province }, function (data) {
            $('#area').empty().append('<option value="">Select Area</option>');
            $.each(data, function (i, area) {
                $('#area').append($('<option>', {
                    value: area,
                    text: area
                }));
            });
        });
    }

    function loadStreets(area, city, province) {
        $.getJSON('/Regions/GetStreets', { area: area, city: city, province: province }, function (data) {
            $('#street').empty().append('<option value="">Select Street</option>');
            $.each(data, function (i, street) {
                $('#street').append($('<option>', {
                    value: street,
                    text: street
                }));
            });
        });
    }

    function loadVillages(street, area, city, province) {
        $.getJSON('/Regions/GetVillages', { street: street, area: area, city: city, province: province }, function (data) {
            $('#village').empty().append('<option value="">Select Village</option>');
            $.each(data, function (i, village) {
                $('#village').append($('<option>', {
                    value: village,
                    text: village
                }));
            });
        });
    }

    $('#province').change(function () {
        var province = $(this).val();
        loadCities(province);
        $('#area, #street, #village').empty().append('<option value="">Select Option</option>');
    });

    $('#city').change(function () {
        var city = $(this).val();
        var province = $('#province').val();
        if (province) {
            loadAreas(city, province);
            $('#street, #village').empty().append('<option value="">Select Option</option>');
        }
    });

    $('#area').change(function () {
        var area = $(this).val();
        var city = $('#city').val();
        var province = $('#province').val();
        if (city && province) {
            loadStreets(area, city, province);
            $('#village').empty().append('<option value="">Select Option</option>');
        }
    });

    $('#street').change(function () {
        var street = $(this).val();
        var area = $('#area').val();
        var city = $('#city').val();
        var province = $('#province').val();
        if(area && city && province)
            loadVillages(street, area, city, province);
    });

    //-----------------------------------------

    $('#province').focus(function () {
        var province = $(this).val();
        loadProvinces();
    });

    $('#city').focus(function () {
        var city = $(this).val();
        var province = $('#province').val();
        if(province)
            loadCities(province);
    });

    $('#area').focus(function () {
        var area = $(this).val();
        var city = $('#city').val();
        var province = $('#province').val();
        if (city && province);
            loadAreas(city, province);
    });

    $('#street').focus(function () {
        var street = $(this).val();
        var area = $('#area').val();
        var city = $('#city').val();
        var province = $('#province').val();
        if (area && city && province)
            loadStreets( area, city, province);
    });
    // Initial load of provinces
});
