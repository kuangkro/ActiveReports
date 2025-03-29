import { createViewer } from './jsViewer.min.js';

function selectReportElement(reportName) {
    const reportsList = document.getElementById("reportsList");
    const reports = reportsList.children;

    for (let i = 0; i < reports.length; i++) {
        if (reports[i].children[0].innerText === reportName)
            reports[i].classList.add('active');
        else
            reports[i].classList.remove('active');
    }
}

function getReports() {
    return fetch("reports")
        .then(response => response.json())
        .catch(error => {
            console.error("Error fetching reports:", error);
            return [];
        });
}

function fillReportsList(reports) {
    const reportsList = document.getElementById("reportsList");
    reportsList.innerHTML = "";
    for (let i = 0; i < reports.length; i++) {
        const reportName = reports[i];
        const reportElement = document.createElement('li');
        reportElement.className = 'navbar-item';
        const title = document.createElement('span');
        title.innerText = reportName;
        reportElement.appendChild(title);
        reportsList.appendChild(reportElement);

        reportElement.addEventListener('click', function () {
            openReport(reportName);
        });
    }
}

function openReport(reportName) {
    const postData = [{
        "productCode": "A001",
        "productName": "apple",
        "price": 33,
        "storeNumber": 200,
        "address": "china"
    }, {
        "productCode": "A002",
        "productName": "apple",
        "price": 33,
        "storeNumber": 200,
        "address": "japan"
    },
    ];

    var cacheKey = "key01";
    fetch("/reportserver/data/cache/" + cacheKey, {
        method: "post",
        header: { "Content-Type": "application/json" },
        body: JSON.stringify(postData)
    }).then(resp => {
        console.log(resp);
        if (!resp.ok) {
            throw new Error("网络响应失败")
        }
        return resp.json();
    }).then(data => {
        console.log(data);

        viewer.openReport(reportName + "$" + cacheKey);
        selectReportElement(reportName);
    })
        .catch(error => {
            console.log("请求异常", error)
        });
}

const viewer = createViewer({
    element: '#viewerContainer',
    reportParameters: [{ name: "out_parameter", values: ["前端回传数据"] }]
});

getReports().then(reports => {
    if (reports.length > 0) {
        fillReportsList(reports);
        //openReport(reports[0]);
    }
});