// データを保持するグローバル変数とグラフ実体
var all_data = {};
var myChart;
// ミリ秒との相互変換用定数
// msec -> sec -> minites
const MSEC_TO_MINS = 60 * 1000;
// msec -> sec -> minites -> hours
const MSEC_TO_HOURS = 24 * 60 * 1000;

// Webページが最初にロードされたとき実行される(初期値に関する処理など)
window.addEventListener('load', function () {
    var init_min_meter = [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0];
    var init_max_meter = [100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100];
    drawGraph(["0:00", "1:00", "2:00", "3:00", "4:00", "5:00", "6:00", "7:00", "8:00", "9:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00"],
        init_min_meter, init_max_meter);
});

function getAPITempHumi(device_id, fromT, toT) {
    // APIサーバのURL
    const api_url = "https://breeding-support-syste.ml:8080/getSensorInfo/";
    var date_from = fromT;
    var date_to = toT;

    const obj = { device_id: device_id, date_from: date_from, date_to: date_to};
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
            return response.json();
            // テキストとして渡す
            //return response.text();
        }

        }).then(recv_json => {
            if (recv_json == '') {
                alert('対象のデータがありません');
                return;
            }
            // データをグローバル変数へ
            all_data = recv_json;
            // チェックボックスの選択されている場所に応じて呼び出し
            call_selected_scale();
    })
        .catch(error_message => {
            alert("GetAPITempHumi" + error_message);
            console.log("GetAPITempHumi");
            console.log(error_message);
        });
}

// 別の選択肢が呼ばれた場合
function click_scale() {
    // デバイスIDを取得
    var device_id = document.getElementById("input_id").value;
    // 選択肢の場所を取得
    var clicked_selected_scale = get_selected_scale();
    // 現在の時刻を取得する
    var current_time = moment().toDate();
    //var current_time = moment("2021-07-28 15:12:00").toDate();

    switch (clicked_selected_scale) {
        case 0:
            var date_from = moment(current_time).add(-12, 'hours').format("YYYY-MM-DD HH:mm:00");
            var date_to = moment(current_time).format("YYYY-MM-DD HH:mm:00");
            // APIから値を取得
            getAPITempHumi(device_id, date_from, date_to);
            break;
        case 1:
            var date_from = moment(current_time).add(-1, 'hours').format("YYYY-MM-DD HH:mm:00");
            var date_to = moment(current_time).format("YYYY-MM-DD HH:mm:00");
            // APIから値を取得
            getAPITempHumi(device_id, date_from, date_to);
            break;
        case 2:
            var date_from = moment(current_time).add(-1, 'days').format("YYYY-MM-DD HH:mm:00");
            var date_to = moment(current_time).format("YYYY-MM-DD HH:mm:00");
            // APIから値を取得
            getAPITempHumi(device_id, date_from, date_to);
            break;
        default:
            alert("設定されていない選択肢です(click_scale)")
            break;
    }
}

// グラフスケールを取得する
function get_selected_scale() {
    // スケールのどれが選択されているか
    if (document.getElementById("graph_scale_12hours").checked) {
        // デフォルト
        return 0;
    }
    else if (document.getElementById("graph_scale_default").checked) {
        // 1時間表示
        return 1;
    }
    else if (document.getElementById("graph_scale_1days").checked) {
        // 1日表示
        return 2;
    }
    else {
        return -1;
    }
}

// グラフスケールを取得して呼び出す
function call_selected_scale() {
    // スケールのどれが選択されているか
    if (document.getElementById("graph_scale_12hours").checked) {
        // 12時間表示
        // 縮小データ
        var reduced_data = [];
        var hours_data = [];

        // 全データを古い方から1時間ずつ分類して見ていく
        var month_cnt = moment(all_data[0].date).hour();
        all_data.forEach(function (element) {
            // 前周回と同じ時間か
            if (month_cnt == moment(element.date).hour()) {
                if ('null' != element.temp) {
                    hours_data.push(element);
                }
            }
            else {
                var sum_month = 0.0;
                var average = null;
                for (var index = 0; index < hours_data.length; index++) {
                    sum_month += Number(hours_data[index].temp);
                }
                
                // 平均温度を計算
                if (hours_data.length !== 0) {
                    average = sum_month / hours_data.length;
                }
                // 一時的に持っていた配列を初期化
                hours_data.length = 0;
                if ('null' != element.temp) {
                    hours_data.push(element);
                }

                // 連想配列を作成{日付:この時間, 気温:平均, 湿度:最終要素}
                var data_obj = {};
                // 1時間前のラベルに代わっているので、ここで調整
                var past_month = moment(element.date).add(-1, 'hours');
                data_obj["date"] = moment(past_month).format("YYYY-MM-DD HH:00:00");
                data_obj["temp"] = average;
                data_obj["humi"] = element.humi;
                reduced_data.push(data_obj);

                // 違う時間であれば、次の一時配列に追加(26時 -> 2時に変換も忘れなく)
                month_cnt += 1;
                month_cnt = month_cnt % 24;
            }
        });

        // 最後の時間が欠損するので、追加
        var final_obj = {};
        final_obj["date"] = moment(all_data[all_data.length - 1].date).format("YYYY-MM-DD HH:mm:00");
        final_obj["temp"] = all_data[all_data.length - 1].temp;
        final_obj["humi"] = all_data[all_data.length - 1].humi;
        reduced_data.push(final_obj);

        // 以上で圧縮を確定して次の操作へ
        slice_data(0, reduced_data);
    }
    else if (document.getElementById("graph_scale_default").checked) {
        // 1時間表示
        slice_data(1, all_data);
    }
    else if (document.getElementById("graph_scale_1days").checked) {
        // 1日表示
        // 縮小データ
        var reduced_data = []; 
        var hours_data = [];

        // 全データを古い方から1時間ずつ分類して見ていく
        var month_cnt = moment(all_data[0].date).hour();
        all_data.forEach(function (element) {
            // 前周回と同じ時間か
            if (month_cnt == moment(element.date).hour()) {
                hours_data.push(element);
            }
            else {
                // 違う時間であれば、次の一時配列に追加(26時 -> 2時に変換も忘れなく)
                month_cnt += 1;
                month_cnt = month_cnt % 24;
                var sum_month = 0.0;
                var average = 0.0;
                for (var index = 0; index < hours_data.length; index++) {
                    sum_month += Number(hours_data[index].temp);
                }
                // 平均温度を計算
                average = sum_month / hours_data.length;
                // 一時的に持っていた配列を初期化
                hours_data.length = 0;
                hours_data.push(element);

                // 連想配列を作成{日付:この時間, 気温:平均, 湿度:最終要素}
                var data_obj = {};
                // 1時間前のラベルに代わっているので、ここで調整
                var past_month = moment(element.date).add(-1, 'hours');
                data_obj["date"] = moment(past_month).format("YYYY-MM-DD HH:00:00");
                data_obj["temp"] = average;
                data_obj["humi"] = element.humi;
                reduced_data.push(data_obj);
            }
        });

        // 最後の時間が欠損するので、追加
        var final_obj = {};
        final_obj["date"] = moment(all_data[all_data.length - 1].date).format("YYYY-MM-DD HH:mm:00");
        final_obj["temp"] = all_data[all_data.length - 1].temp;
        final_obj["humi"] = all_data[all_data.length - 1].humi;
        reduced_data.push(final_obj);

        slice_data(2, reduced_data);
    }
    else {
        slice_data(-1, all_data);
    }
}

// 文字列を日付型として返す関数
function str_to_date(date_str) {
    // 念のためStringに変換
    date_str = String(date_str);
    // 年月日 || 時刻
    var space = date_str.split(' ');
    // 年||月||日
    var minus = space[0].split('-');
    // 時間||分||秒
    var coron = space[1].split(':');

    return new Date(minus[0], minus[1] - 1, minus[2], coron[0], coron[1], coron[2]);
}

// 日付型を文字列として返す関数
function date_to_str(type_date) {
    var year_month_day = type_date.getFullYear() + '-' + (type_date.getMonth() + 1).toString().padStart(2, '0') + '-' + type_date.getDate().toString().padStart(2, '0');
    var hours_min_second = type_date.getHours().toString().padStart(2, '0') + ':' + type_date.getMinutes().toString().padStart(2, '0') + ':' + type_date.getSeconds().toString().padStart(2, '0');
    return year_month_day + ' ' + hours_min_second;
}

// 選択されているラジオボタンによって、描画する範囲を変える関数
// scale : 選択されているスケール
// recv_data : 受け取った全てのデータ
function slice_data(scale, recv_data)
{
    // 比較用の呼び出された現在の時刻 
    var current_dates = moment().format("YYYY-MM-DD HH:mm:00");
    //var current_dates = moment("2021-07-28 15:12:00");
    // グラフ上の左端となる要素
    var left_limit = null;
    var left_limit_index = -1;
    // グラフ上の中心となる要素
    var head_position = null;
    // グラフ上の右端となる要素
    var right_limit;

    // プロットする決められた範囲の全データ
    var plot_data = [];

    switch (scale)
    {
        // 12時間表示モード(左端：現時刻の12時間前, 頭端：存在するデータの最新, 右端：現時刻)
        case 0:
            for (var index = recv_data.length - 1; 0 <= index; index--) {
                // if( 現時刻から一番近い最初の要素だったら)
                // その場所をデータの頭端として保存する
                if (head_position == null) {
                    head_position = recv_data[index];
                }

                // if( 現時刻の1時間前に一番近い要素が見つかったら)
                // その場所を左端として保存する
                var d_recv_data = moment(head_position.date).add(-12, 'hours').format("YYYY-MM-DD HH:mm:00");
                if (moment(recv_data[index].date) <= d_recv_data && left_limit_index === -1) {
                    left_limit_index = index;
                    left_limit = recv_data[index];
                }
                else {
                    // 左端のデータが存在しない場合は、[0](存在するデータの最古のもの)を入れておく
                    left_limit_index = 0;
                    left_limit = recv_data[0];
                }
            }

            // グラフ上でプロットする範囲のデータを定義
            for (var index = left_limit_index; index < recv_data.length; index++) {
                // 前から見て行って、指定した日付の範囲になったらplot_dataとする
                if (left_limit.date <= recv_data[index].date) {
                    plot_data.push(recv_data[index]);
                }
            }


            // グラフ上の右端
            // 当面は右端=ブラウザを開いた時刻とする
            var future_data = {};
            future_data["date"] = current_dates;
            future_data["temp"] = NaN;
            future_data["humi"] = NaN;
            var future_json = JSON.stringify(future_data);
            future_json = JSON.parse(future_json);

            // 以上で範囲の設定は完了したので、指定した範囲のプロットを依頼する
            createPlotData(scale, plot_data, left_limit, head_position, future_json);
            break;

        // 1時間表示モード(左端：データの1時間前, 頭端：存在するデータの最新, 右端：現在時刻)
        case 1:
            // 最新側から探索
            for (var index = recv_data.length - 1; 0 <= index; index--) {
                // if( 現時刻から一番近い最初の要素だったら)
                // その場所をデータの頭端として保存する
                if (head_position == null) {
                    head_position = recv_data[index];
                }
                
                // if( 現時刻の1時間前に一番近い要素が見つかったら)
                // その場所を左端として保存する
                var d_recv_data = moment(head_position.date).add(-1, 'hours').format("YYYY-MM-DD HH:mm:00");
                if (moment(recv_data[index].date) <= d_recv_data && left_limit_index === -1)
                {
                    left_limit_index = index;
                    left_limit = recv_data[index];
                }
                else
                {
                    // 左端のデータが存在しない場合は、[0](存在するデータの最古のもの)を入れておく
                    left_limit_index = 0;
                    left_limit = recv_data[0];
                }
            }

            // グラフ上でプロットする範囲のデータを定義
            for (var index = left_limit_index; index < recv_data.length; index++) {
                // 前から見て行って、指定した日付の範囲になったらplot_dataとする
                if (left_limit.date <= recv_data[index].date) {
                    plot_data.push(recv_data[index]);
                }
            }
            

            // グラフ上の右端
            // 当面は右端=ブラウザを開いた時刻とする
            var future_data = {};
            future_data["date"] = current_dates;
            future_data["temp"] = NaN;
            future_data["humi"] = NaN;
            var future_json = JSON.stringify(future_data);
            future_json = JSON.parse(future_json);

            // 以上で範囲の設定は完了したので、指定した範囲のプロットを依頼する
            createPlotData(scale, plot_data, left_limit, head_position, future_json);
            break;
        case 2:
            // 1日分を表示
            for (var index = recv_data.length - 1; 0 <= index; index--) {
                // if( 現時刻から一番近い最初の要素だったら)
                // その場所をデータの頭端として保存する
                if (head_position == null) {
                    head_position = recv_data[index];
                }

                // if( 現時刻の1時間前に一番近い要素が見つかったら)
                // その場所を左端として保存する
                var d_recv_data = moment(head_position.date).add(-24, 'hours').format("YYYY-MM-DD HH:mm:00");
                if (moment(recv_data[index].date) <= d_recv_data && left_limit_index === -1) {
                    left_limit_index = index;
                    left_limit = recv_data[index];
                }
                else {
                    // 左端のデータが存在しない場合は、[0](存在するデータの最古のもの)を入れておく
                    left_limit_index = 0;
                    left_limit = recv_data[0];
                }
            }

            // グラフ上でプロットする範囲のデータを定義
            for (var index = left_limit_index; index < recv_data.length; index++) {
                // 前から見て行って、指定した日付の範囲になったらplot_dataとする
                if (left_limit.date <= recv_data[index].date) {
                    plot_data.push(recv_data[index]);
                }
            }


            // グラフ上の右端
            // 当面は右端=ブラウザを開いた時刻とする
            var future_data = {};
            future_data["date"] = current_dates;
            future_data["temp"] = NaN;
            future_data["humi"] = NaN;
            var future_json = JSON.stringify(future_data);
            future_json = JSON.parse(future_json);

            // 以上で範囲の設定は完了したので、指定した範囲のプロットを依頼する
            createPlotData(scale, plot_data, left_limit, head_position, future_json);
            break;
        default:
            alert("異例の選択肢が選ばれました(slice_data)");
            break;
    }
}

// データの中から指定した範囲のデータを選んでプロットを要求
// scale : 選択中のスケール
// data_list : 検索する全データ(date, temp, humiを含む)
// left_date : グラフの左端となるデータ
// head_date : 存在するデータのうちの最新のデータ
// right_date : グラフの右端となるデータ(未来の存在しない日でもよい)

function createPlotData(scale, data_list, left_date, head_date, right_date)
{
    // 描画用関数に渡すリスト
    var data_label = [];
    var temp_data = [];
    var humi_data = [];

    // 引数で与えられた全データに対して
    for (var index = 0; index < data_list.length; index++)
    {
        // 日付に欠損値がなければ
        if (data_list[index].date != null) {
            //データを入れ込む
            switch (scale)
            {
                // おすすめモード(直近12時間分を表示) M/D H:MM のみ表示
                case 0:
                    data_label.push(moment(data_list[index].date).format("HH:mm"));
                    break;
                // 1時間分の表示モード H:MM のみ表示
                case 1:
                    data_label.push(moment(data_list[index].date).format("H:mm"));
                    break;
                // 1日分の表示モード H:MM のみ表示
                case 2:
                    data_label.push(moment(data_list[index].date).format("H:mm"));
                    break;
                default:
                    data_label.push(data_list[index].date);
                    break;
            }
            temp_data.push(data_list[index].temp);
            humi_data.push(data_list[index].humi);
        }
        else
        {
            console.log('欠損値あり' + index + '番目のデータ');
        }
    }
    
    // 以上を踏まえてグラフにプロットする
    drawGraph(data_label, temp_data, humi_data);
}


// 与えられた要素を基にグラフを生成する関数
// data_label : 下に表示される日付のラベルのデータ
// temp_data : 日付と対応する温度のデータ
// humi_data : 日付と対応する湿度のデータ
// min_threshold : 閾値の下限
// max_threshold : 閾値の上限
function drawGraph(data_label, temp_data, humi_data)
{
    // グラフが存在していれば一度消す
    if (myChart) {
        myChart.destroy();
    }

    // グラフ上に表示する閾値
    var low_temp = document.getElementById("low_temp").value;
    var high_temp = document.getElementById("high_temp").value;
    var low_humi = document.getElementById("low_humi").value;
    var high_humi = document.getElementById("high_humi").value;
    // 閾値用のデータセットを作る
    var low_temps = [];
    var high_temps = [];
    var low_humis = [];
    var high_humis = [];
    for (var ind = 0; ind < data_label.length; ind++) {
        low_temps.push(low_temp);
        high_temps.push(high_temp);
        low_humis.push(low_humi);
        high_humis.push(high_humi);
    }

    // グラフオプション
    var graph_option =
    {
        responsive: true,
        spanGaps: true,
        scales: {
            yAxes: [{
                // 温度軸
                id: "y-axis-1",
                type: "linear",   
                position: "left", 
                scaleLabel: {
                    display: true,
                    labelString:'Temperature',
                },
                ticks: {          
                    max: 50,
                    min: 0,
                    stepSize: 1,
                    // 軸を間引き
                    callback: function (value, index, values) {
                        if (index % 2 === 1) {
                            return "";
                        }
                        return value;
                    }
                },
            },
                {
                // 湿度軸
                id: "y-axis-2",
                type: "linear",
                position: "right",
                scaleLabel: {
                    display: true,
                    labelString: 'Humidity',
                },
                gridLines: {
                    display: false,
                },
                ticks: {
                    max: 100,
                    min: 0,
                    stepSize: 5,
                },
            }
            ],
        }
    }
    myChart = new Chart(document.getElementById("mainChart"), {
        type: 'bar',
        options: graph_option,
        data:
        {
            labels: data_label,
            datasets: [{
                    type: "line",
                    label: "Temperature",
                    data: temp_data,
                    // 線の以下を塗らない
                    fill: false,
                    // ベジエ曲線でゆるっと繋ぐ
                    lineTension: 0.2,
                    backgroundColor: [
                        "rgba(255, 99, 132, 0.2)"
                    ],
                    borderColor: 'rgba(255, 99, 132, 1)',
                    pointBackgroundColor: 'rgba(255, 99, 132, 1)',
                    yAxisID: "y-axis-1",
                    borderWidth: 5,
            }, {
                    type: "line",
                    label: "TempUpper",
                    data: high_temps,
                    // 線の以下を塗らない
                    fill: false,
                    // ベジエ曲線でゆるっと繋ぐ
                    lineTension: 0.2,
                    backgroundColor: [
                        "rgba(255, 99, 132, 0.6)"
                    ],
                    // 点無し
                    radius: 0,
                    borderColor: 'rgba(255, 149, 132, 1)',
                    borderWidth: 0.6,
                    pointBackgroundColor: 'rgba(255, 99, 132, 1)',
                    yAxisID: "y-axis-1",
                    borderWidth: 2,
            }, {
                    type: "line",
                    label: "TempLower",
                    data: low_temps,
                    // 線の以下を塗らない
                    fill: false,
                    // ベジエ曲線でゆるっと繋ぐ
                    lineTension: 0.2,
                    backgroundColor: [
                        "rgba(255, 99, 132, 0.6)"
                    ],
                    // 点無し
                    radius: 0,
                    borderColor: 'rgba(255, 199, 132, 1)',
                    borderWidth: 0.6,
                    pointBackgroundColor: 'rgba(255, 99, 132, 1)',
                    yAxisID: "y-axis-1",
                    borderWidth: 2,
            }, {
                    type: "line",
                    label: "Humidity", 
                    data: humi_data,
                    // 線の以下を塗らない
                    fill: false,
                    // ベジエ曲線でゆるっと繋ぐ
                    lineTension: 0.2,
                    backgroundColor: [
                       "rgba(255, 99, 132, 0.2)"
                    ],
                    borderColor: 'rgba(54, 102, 235, 1)',
                    pointBackgroundColor: '(54, 102, 235, 1)',
                    pointStyle: 'rect',
                // 右の軸を使う
                yAxisID: "y-axis-2",

            }, {
                    type: "line",
                    label: "HumiUpper",
                    data: high_humis,
                    // 線の以下を塗らない
                    fill: false,
                    // ベジエ曲線でゆるっと繋ぐ
                    lineTension: 0.2,
                    backgroundColor: [
                        "rgba(255, 99, 132, 0.2)"
                    ],
                    // 点無し
                    radius: 0,
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth:0.8,
                    pointBackgroundColor: 'rgba(54, 162, 235, 1)',
                    pointStyle: 'rect',
                    // 右の軸を使う
                    yAxisID: "y-axis-2",
            }, {
                    type: "line",
                    label: "HumiLower",
                    data: low_humis,
                    // 線の以下を塗らない
                    fill: false,
                    // ベジエ曲線でゆるっと繋ぐ
                    lineTension: 0.2,
                    backgroundColor: [
                        "rgba(255, 99, 132, 0.2)"
                    ],
                    // 点無し
                    radius: 0,
                    borderColor: 'rgba(134, 142, 245, 1)',
                    borderWidth: 0.8,
                    pointBackgroundColor: 'rgba(104, 152, 255, 1)',
                    pointStyle: 'rect',
                    // 右の軸を使う
                    yAxisID: "y-axis-2",
            }
                    ]
        },
    });

};
