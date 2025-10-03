def create(): #создать
    note = input("Введите текст заметки: ")
    with open("notes.txt", "a", encoding="utf-8") as file:
        file.write(note + "\n")
    print("Заметка успешно добавлена!")
    print()



def delete(): #удалить
    with open("notes.txt", "r", encoding="utf-8") as file:
            notes = file.readlines()
        
    if not notes:
        print("Список заметок пуст.")
    else:
        show()
        note_num = int(input("Введите номер заметки для удаления: "))
        if 1 <= note_num <= len(notes):
            del notes[note_num - 1]
            
            with open("notes.txt", "w", encoding="utf-8") as file:
                file.writelines(notes)
            print("Заметка успешно удалена.")
        else:
            print("Неверный номер заметки.")
    print()


def search(): #найти
    with open("notes.txt", "r", encoding="utf-8") as file:
            notes = file.readlines()
        
    if not notes:
        print("Список заметок пуст.")
    else:
        keyword = input("Введите текст для поиска: ").lower()
        found = []
        
        for i, note in enumerate(notes, 1):
            if keyword in note.lower():
                found.append((i, note))
        
        if found:
            print("Найденные заметки:")
            for i, text in found:
                print(f"{i}: {text}")
        else:
            print("Заметки не найдены.")
        print()


def close(): #закрыть
    print("До свидания.")
    exit()


def show(): # показать
    with open("notes.txt", "r", encoding="utf-8") as file:
            notes = file.readlines()
        
    if not notes:
        print("Список заметок пуст.")
    else:
        print("Текущие заметки:")
        for i, note in enumerate(notes, 1):
            print(f"{i}:  {note}")
    print()

def interface(): # интерфейс
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

interface()
