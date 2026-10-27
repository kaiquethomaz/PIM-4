// Botão "olhinho": mostra / oculta a senha digitada.
// Funciona em todos os campos de senha das telas de Login e Cadastro.
// aria-pressed e aria-label avisam o leitor de tela se a senha está visível.
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".botao-ver-senha").forEach(function (botao) {
        var campo = document.getElementById(botao.getAttribute("aria-controls"));
        if (!campo) return;

        botao.addEventListener("click", function () {
            var mostrando = campo.type === "text";
            campo.type = mostrando ? "password" : "text";
            botao.setAttribute("aria-pressed", mostrando ? "false" : "true");
            botao.setAttribute("aria-label", mostrando ? "Mostrar senha" : "Ocultar senha");
            campo.focus();
        });
    });
});
