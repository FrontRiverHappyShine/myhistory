// Service worker

// 通知受信時の動作
self.addEventListener('push', function(event) {
    console.log('[notifications] Service Worker: Received');
    msg = event.data.text()
    console.log(msg);

    const title = 'Breeding Support System';
    const options = {
      body: msg,
    };

    event.waitUntil(self.registration.showNotification(title, options));
});

// 通知クリック時の動作
self.addEventListener('notificationclick', function(event) {
    console.log('[notifications] Service Worker: clicked');

    event.notification.close();
    event.waitUntil(
        clients.openWindow('https://breeding-support-syste.ml/')
    );
});
