//$(document).ready(function () {
//    // 获取数据的 URL，假设 URL 是 /Home/RegionNames
//    $.getJSON('/Home/RegionNames', {person : person}, function (data) {
//        // 确保数据有效
//        if (data) {
//            // 拼接字符串
//            var regionString = [
//                'Province: ' + (data.ProvinceName || 'N/A'),
//                'City: ' + (data.CityName || 'N/A'),
//                'Area: ' + (data.AreaName || 'N/A'),
//                'Street: ' + (data.StreetName || 'N/A'),
//                'Village: ' + (data.VillageName || 'N/A')
//            ].join(', ');

//            // 将拼接的字符串传递给 HTML 元素
//            $('#regionInfo').text(regionString);
//        }
//    });
//});
