
// 設定
const notificationPublicKey = 'BDDYrNyuihTOpDj-tniIuT3nfVkBstR2UQFq3jrBOHJrVjJ_A_hD5utyGl8pLPpNtHkvt0bcbQGG-f2iaCZ1Rno';

let pushButton = null;
let isSubscribed = false;
let swRegistration = null;

// 初期化処理
window.addEventListener('load', function(){
  // Service Worker の登録とボタンの初期化
  pushButton = document.querySelector('.js-push-btn');
  if ('serviceWorker' in navigator && 'PushManager' in window) {
    navigator.serviceWorker.register('/js/notification_sw.js')
      .then(function(swReg) {
        console.log('[notifications] Service Worker: registered', swReg);
        swRegistration = swReg;
        initialiseUI();
      })
      .catch(function(error) {
        console.error('[notifications] Service Worker: Error ', error);
      });
  } else {
    pushButton.textContent = '通知機能がブラウザでサポートされていません';
  }
});

// ボタンの状態を更新する関数
function updateNotificationBtn() {
  if (Notification.permission === 'denied') {
    pushButton.textContent = '通知がブロックされています';
    pushButton.disabled = true;
    return;
  }

  if (isSubscribed) {
    pushButton.textContent = '通知機能を無効化する';
  } else {
    pushButton.textContent = '通知機能を有効化する';
  }
  pushButton.disabled = false;
}

// UIの初期化を行う関数
function initialiseUI() {
  // ボタンクリック時に動作するイベントの追加
  pushButton.addEventListener('click', function() {
    // 登録処理または登録解除処理の実行
    pushButton.disabled = true;
    if (isSubscribed) {
      unsubscribeUser();
    } else {
      subscribeUser();
    }
  });

  // 通知端末が登録済みであるか確認
  swRegistration.pushManager.getSubscription().then(function(subscription) {
    // 現在の状態を保持
    isSubscribed = !(subscription === null);
    // デバッグログ
    if (isSubscribed) {
      console.log('[notifications] initialiseUI: subscribed');
    } else {
      console.log('[notifications] initialiseUI: not subscribed');
    }
    // ボタンの状態を更新
    updateNotificationBtn();
  });
}

// 通知端末の登録処理を行う関数
function subscribeUser() {
  const applicationServerKey = urlB64ToUint8Array(notificationPublicKey);
  swRegistration.pushManager.subscribe({
    userVisibleOnly: true,
    applicationServerKey: applicationServerKey
  })
  .then(function(subscription) {
    console.log('[notifications] subscribeUser: subscribed ', subscription);
    // サーバーに通知端末の登録をリクエスト
    updateSubscriptionOnServer(subscription, false);
    // 状態の更新
    isSubscribed = true;
    updateNotificationBtn();
  })
  .catch(function(err) {
    console.log('[notifications] subscribeUser: Failed to subscribe ', err);
    updateNotificationBtn();
  });
}

// 通知端末の登録解除処理を行う関数
function unsubscribeUser() {
  swRegistration.pushManager.getSubscription()
  .then(function(subscription) {
    if (subscription) {
      subscription.unsubscribe();
      // サーバーに通知端末の登録解除をリクエスト
      updateSubscriptionOnServer(subscription, true);
      return;
    }
  })
  .catch(function(error) {
    console.log('[notifications] Error unsubscribing', error);
  })
  .then(function() {
    console.log('[notifications] User is unsubscribed.');
    // 状態の更新
    isSubscribed = false;
    updateNotificationBtn();
  });
}

// サーバーに通知端末の登録や登録解除をリクエストする関数
function updateSubscriptionOnServer(subscription, is_unsubscribe) {
  if (subscription) {
    subscription_json_data = JSON.stringify(subscription);
    console.log('[notification] updateSubscriptionOnServer:<response> ' + subscription_json_data);

    const url_path = !is_unsubscribe ? "/subscribeNotification/" : "/unsubscribeNotification/";
    const url = 'https://breeding-support-syste.ml:8080' + url_path;
    const obj = {endpoint: subscription_json_data};
    const body_data = Object.keys(obj).map((key) => key + "=" + encodeURIComponent(obj[key])).join("&");

    fetch(url, {
      method: 'post',
      mode: 'cors',
      cache: 'no-cache',
      headers: new Headers({
        'Content-Type': 'application/x-www-form-urlencoded; charset=utf-8'
      }),
      body: body_data,
    }).then(function(response) {
      return response.text();
    }).then(function(response) {
      console.log('[notifications] updateSubscriptionOnServer:<response> ' + response);
    }).catch(function(err) {
      console.log('[notifications] updateSubscriptionOnServer:<Error> ' + err);
    });
  }
}

// applicationServerKey用の変換関数
function urlB64ToUint8Array(base64String) {
  const padding = '='.repeat((4 - base64String.length % 4) % 4);
  const base64 = (base64String + padding).replace(/\-/g, '+').replace(/_/g, '/');

  const rawData = window.atob(base64);
  const outputArray = new Uint8Array(rawData.length);

  for (let i = 0; i < rawData.length; ++i) {
    outputArray[i] = rawData.charCodeAt(i);
  }
  return outputArray;
}
