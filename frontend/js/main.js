const translateButton = document.getElementById('translate-btn');
const loadHistoryButton = document.getElementById('load-btn');
const textInput = document.getElementById('text-input');
const textOutput = document.getElementById('text-output');
const langIn = document.getElementById('select-lang-in');
const langOut = document.getElementById('select-lang-out');
const historyContainer = document.getElementById('history-container');
let history = "";
let lastId;
let newId;
let languages;
let lastRecord;
let firstOne = true;

const showHistortRecords = () => {

    if (history === "" || lastId == 0) return;
    //creating new DOM objects for a history 
    console.log("called");

    history.forEach(record => {
        if (record._id <= lastId && record._id > (lastId - 5)) {
            console.log(record._id);

            const lang1 = Object.entries(languages).filter(([key, value]) => { if (key === record.lang_in) return value })[0][1].name;
            const lang2 = Object.entries(languages).filter(([key, value]) => { if (key === record.lang_out) return value })[0][1].name;

            let newRecord = document.createElement("div");
            newRecord.classList.add("history-record");
            let newList = document.createElement("ul");
            newList.classList.add("history-record-desc");
            let newItem1 = document.createElement("li");
            newItem1.classList.add("history-langs");
            newItem1.innerText = `${record._id + 1}. ${lang1} ---> ${lang2}`;
            let newItem2 = document.createElement("li");
            newItem2.classList.add("history-lang1");
            newItem2.innerText = record.text_in;
            let newItem3 = document.createElement("li");
            newItem3.classList.add("history-lang2");
            newItem3.innerText = record.text_out;
            newList.appendChild(newItem1);
            newList.appendChild(newItem2);
            newList.appendChild(newItem3);
            newRecord.appendChild(newList);
            historyContainer.append(newRecord);
            if (firstOne) { lastRecord = newRecord; firstOne = false }
        }
    });
    lastId = lastId - 5;
    if (lastId < 1) loadHistoryButton.remove();
}

const azureTranslate = (lang_in, lang_out, text_in) => {
    translationReq = `{"lang_in": "${lang_in}", "lang_out": "${lang_out}", "text_in": "${text_in}"}`;

    return fetch("http://localhost:7071/api/RunTranslation", {
        method: "POST",
        mode: "cors",
        headers: {
            "Content-Type": "application/json",
        },
        body: translationReq,
    })
        .then(response => {

            if (!response.ok) {
                throw new Error("Response error");
            }
            return response.json();
        })
        .then((data) => {
            console.log(data);
            return data.text_out;
        })
        .catch(error => {
            console.error('Error azureTranslate:', error);
        });

}

const runTranslation = () => {
    //if the target and the source lang are the same or the input is empty don`t call azure functions
    if (langIn.value === langOut.value || textInput.value === "") {
        textOutput.value = textInput.value;
    }
    else {
        azureTranslate(langIn.value, langOut.value, textInput.value)
        .then(result => {
            newId = newId + 1;
            let newRecord = document.createElement("div");
            newRecord.classList.add("history-record");
            let newList = document.createElement("ul");
            newList.classList.add("history-record-desc");
            let newItem1 = document.createElement("li");
            newItem1.classList.add("history-langs");
            newItem1.innerText = `${newId + 1}. ${langIn.selectedOptions[0].firstChild.data} ---> ${langOut.selectedOptions[0].firstChild.data}`;
            let newItem2 = document.createElement("li");
            newItem2.classList.add("history-lang1");
            newItem2.innerText = textInput.value;
            let newItem3 = document.createElement("li");
            newItem3.classList.add("history-lang2");
            newItem3.innerText = result;
            newList.appendChild(newItem1);
            newList.appendChild(newItem2);
            newList.appendChild(newItem3);
            newRecord.appendChild(newList);
            historyContainer.insertBefore(newRecord, lastRecord);
            lastRecord = newRecord;
            textOutput.value = result;
        })
        .catch(error => {
            console.error("Error:", error);
        });
    }
}

const loadInit = () => {

    //load avaliable languages
    fetch('http://127.0.0.1:3000/frontend/json/languages.json')
        .then(response => {

            if (!response.ok) {
                throw new Error("Response error");
            }
            return response.json();
        })
        .then(data => {
            languages = data.translation;
            Object.entries(languages).forEach(([key, value]) => addLanguage(value.name, key));
            langIn.value = 'hu';
            langOut.value = 'en';
        })
        .catch(error => {
            console.error('Error fetching avaliable languages:', error);
        });

    //load history
    fetch('http://127.0.0.1:3000/frontend/json/history.json')
        .then(response => {

            if (!response.ok) {
                throw new Error("Response error");
            }
            return response.json();
        })
        .then(data => {
            history = data;
            lastId = history.findLastIndex(record => record);
            newId = lastId;
            history = history.reverse();
            showHistortRecords(5);

        })
        .catch(error => {
            console.error('Error fetching history:', error);
        });

}


const addLanguage = (langName, langValue) => {

    let newLang = document.createElement("option");
    newLang.value = langValue;
    newText = document.createTextNode(langName);
    newLang.appendChild(newText);
    langIn.append(newLang);

    newLang = document.createElement("option");
    newLang.value = langValue;
    newText = document.createTextNode(langName);
    newLang.appendChild(newText);
    langOut.append(newLang);

    console.log(langName, langValue)
}

translateButton.addEventListener("click", runTranslation);
loadHistoryButton.addEventListener("click", showHistortRecords);
document.addEventListener('DOMContentLoaded', loadInit);