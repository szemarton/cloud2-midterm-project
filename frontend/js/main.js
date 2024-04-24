const translateButton = document.querySelector('.translate-btn');
const textInput = document.querySelector('.text-input');
const textOutput = document.querySelector('.text-output');
const langIn = document.querySelector('.select-lang-in');
const langOut = document.querySelector('.select-lang-out');

function translate(){
    let message = Math.random() + "TRANSLATE";
    console.log(message);
    textOutput.value = message;

}

translateButton.addEventListener("click", translate);