// SignalR ile gerçek zamanlı güncelleme
// API base url'ini config'den veya window'dan al
var apiBaseUrl = window.apiBaseUrl || 'http://localhost:5000/api/';
var apiRoot = apiBaseUrl.replace(/\/api\/?$/, '');
const connection = new signalR.HubConnectionBuilder()
    .withUrl(apiRoot + '/hubs/notification', {
        withCredentials: false
    })
    .build();

connection.on('RoutineUpdated', function (message) {
    // Burada UI'da güncelleme yapılabilir
    console.log('Rutin güncellendi: ' + message);
});

connection.start().catch(function (err) {
    console.error('SignalR bağlantı hatası:', err.toString());
}); 