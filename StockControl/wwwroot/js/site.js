// Menu lateral no celular: abre e fecha como uma "gaveta".
// - aria-expanded avisa o leitor de tela se o menu está aberto;
// - a tecla Esc e o clique no fundo escuro fecham o menu;
// - ao abrir, o foco vai para o botão de fechar (navegação por teclado).
document.addEventListener("DOMContentLoaded", function () {
    var botaoAbrir = document.getElementById("botao-menu");
    var botaoFechar = document.getElementById("botao-fechar-menu");
    var fundo = document.getElementById("sidebar-fundo");
    if (!botaoAbrir || !botaoFechar || !fundo) return;

    function abrir() {
        document.body.classList.add("menu-aberto");
        botaoAbrir.setAttribute("aria-expanded", "true");
        fundo.hidden = false;
        botaoFechar.focus();
    }

    function fechar() {
        document.body.classList.remove("menu-aberto");
        botaoAbrir.setAttribute("aria-expanded", "false");
        fundo.hidden = true;
        botaoAbrir.focus();
    }

    botaoAbrir.addEventListener("click", abrir);
    botaoFechar.addEventListener("click", fechar);
    fundo.addEventListener("click", fechar);
    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape" && document.body.classList.contains("menu-aberto")) fechar();
    });
});
