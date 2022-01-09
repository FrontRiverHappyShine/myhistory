function getAPINotification(device_id) {
    // APIサーバのURL
    const api_url = "https://breeding-support-syste.ml:8080/getNotificationHistory/";
    const data_num = 3;
    const obj = { device_id: device_id, num: data_num };
    const body_data = Object.keys(obj).map((key) => key + "=" + encodeURIComponent(obj[key])).join("&");

    // APIサーバからデータの取得
    fetch(api_url, {
        method: 'POST',
        mode: 'cors',
        cache: 'no-cache',
        headers: new Headers({
            'Content-Type': 'application/x-www-form-urlencoded; charset=utf-8'
        }),
        body: body_data,
    }).then(response => {

        // 通信エラー
        if (!response.ok) {
            alert("通信エラーが発生しました、ネットワーク接続を確認してください")
        }
        else {
            // テキストの場合
            //return response.text();
            if (response == '') {
                alert('There is no record of the notification')
                return
            }
            else {
                return response.json();
            }
        }

    }).then(recv_json => {
        drawNotification(recv_json);
    })
        .catch(error_message => {
            alert("GetAPINotification" + error_message);
        });
}

function drawNotification(recv_value) {
    // 長さチェック
    if (recv_value.length == 1) {
        // 一番上の通知
        document.getElementById("notifi1_state").innerText = status_to_str(recv_value[0].status);
        document.getElementById("notifi1_date").innerText = recv_value[0].date;
        document.getElementById("notifi1_msg").innerText = recv_value[0].msg;
    }
    else if (recv_value.length == 2) {
        // 一番上の通知
        document.getElementById("notifi1_state").innerText = status_to_str(recv_value[0].status);
        document.getElementById("notifi1_date").innerText = recv_value[0].date;
        document.getElementById("notifi1_msg").innerText = recv_value[0].msg;
        // 中段の通知
        document.getElementById("notifi2_state").innerText = status_to_str(recv_value[1].status);
        document.getElementById("notifi2_date").innerText = recv_value[1].date;
        document.getElementById("notifi2_msg").innerText = recv_value[1].msg;
    }
    else if (recv_value.length == 3) {
        // 一番上の通知
        document.getElementById("notifi1_state").innerText = status_to_str(recv_value[0].status);
        document.getElementById("notifi1_date").innerText = recv_value[0].date;
        document.getElementById("notifi1_msg").innerText = recv_value[0].msg;
        // 中段の通知
        document.getElementById("notifi2_state").innerText = status_to_str(recv_value[1].status);
        document.getElementById("notifi2_date").innerText = recv_value[1].date;
        document.getElementById("notifi2_msg").innerText = recv_value[1].msg;
        // 最下部の通知
        document.getElementById("notifi3_state").innerText = status_to_str(recv_value[2].status);
        document.getElementById("notifi3_date").innerText = recv_value[2].date;
        document.getElementById("notifi3_msg").innerText = recv_value[2].msg;
    }
    else {
        alert("The amount of notifications you have requested is too large(drawNotification)")
    }
};


function status_to_str(status_code) {
    var status = Number(status_code);
    switch (status) {
        case -2: return "危険(Serious Danger)"; break;
        case -1: return "注意(Caution)"; break;
        case 0: return "正常範囲(Normal)"; break;
        case 1: return "注意(Caution)"; break;
        case 2: return "危険(Serious Danger)"; break;
        default: return "異常(ABNORMALITY)"; break;
    }
}
