// Abre e fecha o menu no celular.
// aria-expanded avisa o leitor de tela se o menu está aberto ou fechado.
document.addEventListener("DOMContentLoaded", function () {
    var botao = document.getElementById("botao-menu");
    var cabecalho = document.querySelector(".cabecalho");
    if (!botao || !cabecalho) return;

    botao.addEventListener("click", function () {
        var aberto = cabecalho.classList.toggle("menu-aberto");
        botao.setAttribute("aria-expanded", aberto ? "true" : "false");
    });
});
