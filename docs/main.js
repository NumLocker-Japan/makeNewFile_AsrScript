document.addEventListener('DOMContentLoaded', () => {

    document.querySelector('#open_menu').addEventListener('click', () => {
        document.querySelector('#index').classList.toggle('isInvisible');
    })
    
    document.querySelector('#index_sub').addEventListener('click', () => {
        document.querySelector('#index').classList.add('isInvisible');
    })
    
    document.querySelector('#close_menu').addEventListener('click', () => {
        document.querySelector('#index').classList.add('isInvisible');
    })
    
    document.querySelectorAll('#index_main a').forEach(x => {
        x.addEventListener('click', () => {
            document.querySelector('#index').classList.add('isInvisible');
        })
    })
    
    document.querySelector('#select_color_theme').addEventListener('change', event => {
        if (event.target.selectedIndex == 0)
        {
            document.querySelector('body').setAttribute('color_theme', 'light');
        }
        else if (event.target.selectedIndex == 1)
        {
            document.querySelector('body').setAttribute('color_theme', 'dark');
        }
        else if (event.target.selectedIndex == 2)
        {
            document.querySelector('body').setAttribute('color_theme', 'high_contrast');
        }
    })

})
