def create(): #создать
    pass


def delete(): #удалить
    pass


def search(): #найти
    pass


def close(): #закрыть
    pass


def show(): # показать
    pass

def inteface(): # интерфейс
    print("Дорогой пользователь, приветствуем вас в программе manager of projects. Здесь вы можете работать со своими заметками.")
    while(True):
        print('''Что вы хотите сделать?
              1 - добавить заметку
              2 - удалить заметку
              3 - найти заметку
              4 - показать все заметки
              5 - выйти
              Для выбора команды напишите номер команды.''')
        answer = input()
        match answer:
            case "1": 
                create()

            case "2": 
                delete()

            case "3": 
                search()

            case "4": 
                show()

            case "5": 
                close()
            
            case _: 
                print("Напишите номер команды от 1 до 5. Повторите попытку.")
                continue
